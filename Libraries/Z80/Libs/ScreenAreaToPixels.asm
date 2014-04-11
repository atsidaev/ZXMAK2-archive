;
; Routine will test generated pixels created by program at adress 25000(see below)
;
                org  60000
                di
                ld   hl, #5C8F  
                ld   (hl), 7 ; we have to set correct ink and paper for PLOT command
                ld   hl, #0112
                ld   (PixelCounter), hl
                ld   hl, CoorsOutput
nextPlot        ld   c, (hl)
                inc  hl
                ld   b, (hl)
                inc  hl
                push hl
                push af
                ;call #22E5   ;; PLOT for coordinates X=C, Y=B
                ld   a, PLOT_Y_MAX 
                sub  b
                ld   b, a  ; Y(for PLOT)
                call PixelToScrAdress
                inc  a
                ld   b, a
                xor  a
                scf            ; invert pixel
                rra
                djnz  $-1
                xor  (hl)      ; invert pixel
                ld   (hl), a
                pop  af
                pop  hl
                ld   de, (PixelCounter)
                dec  de
                ld   a, d
                or   e
                jr   z, EndPixelPrint
                ld   (PixelCounter), de
                ;ei
                ;halt
                ;di
                jr   nextPlot
EndPixelPrint:
                ei
                ret
PixelCounter:
DEFW #0112

;
; Desc: Saves pixels on screen into memory. 
;       Coordinates then can be used by PLOT routine(#22E5)
;
; Input: IX - pointer where the coors(X,Y) will be saved
;        HL - screen location where to start with pixeling
;        BC - width and height of the area to be converted into pixels
;
                org  25000
                ld   ix, CoorsOutput
                ld   bc, #3907        ; start position in [Y, X]
                call PixelToScrAdress ; convert it to screen address(HL registry)
                ld   (ScreenPointerSave), hl
                ; real PLOT coordinates
                ld   a, c
                ld   (XCoorStartSave), a
                ld   d, a   ; X(for PLOT)
                ld   a, PLOT_Y_MAX 
                sub  b
                ld   e, a  ; Y(for PLOT)
                ; real PLOT coordinates, end
                ld   bc, #0615 ; [width, height] of the area
                ld   (AreaSizeSave), bc
cycle:          call doOnePixelPosition
                dec  b
                jr   z, nextLine
                inc  hl
                jr   cycle
nextLine
                ld   a, c
                or   a
                ret  z ; end of program
                dec  a
                ld   (AreaSizeSave), a
                ld   c, a
                ld   a, (AreaSizeSave+1)
                ld   b, a
                
                ld   hl, (ScreenPointerSave)
                call IncYCoor ; move screen pointer down one line
                ld   (ScreenPointerSave), hl

                dec  e
                ld   a, (XCoorStartSave) ; restore X plot coordinate
                ld   d, a
                jr   cycle
doOnePixelPosition:
                ld   a, (hl)
                cpl  ; this can be commented out. Here it inverts the original pixels
                or   a
                jr   nz, $+7 ; if A != 0 => make pixels
                ld   a, d
                add  a, 8
                ld   d, a
                ret
                push bc
                ld   b, 8 ; 8 pixels
bitCycle:       rl   a
                jr   nc, nextBit
                ; write PLOT coors
                ld   (ix), d  ; X coor
                inc  ix
                ld   (ix), e  ; Y coor
                inc ix
                ; write PLOT coors end
nextBit
                inc  d ; X for PLOT
                djnz bitCycle
                pop  bc
                ret
; includes
include "IncYCoor.asm"
include "PixelToScrAdress.asm"
include "Constants.lib"

; screen pointer save(HL registry)
ScreenPointerSave:
DEFW 0

; width and height of the area
AreaSizeSave:
DEFW 0

; start X(in pixels) position of the area
XCoorStartSave:
DEFB 0

CoorsOutput: