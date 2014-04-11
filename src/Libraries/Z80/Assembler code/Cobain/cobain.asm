                org  25000
                di
                ld   hl, #5C8F
                ld   (hl), #18 ; we have to set correct ink and paper for PLOT command
                ld   bc, #1800
                ld   (stackSave), sp
                ld   sp, RandomPixels
                ; draw start
cycle:          pop  de
                ld   hl, picCobain
                add  hl, de
                ld   a, (hl)
                ld   hl, SCREEN_START
                add  hl, de
                ld   (hl), a
                ; draw stop
                
                ; halt for some time
                ld   (BCRegSave), BC
                ld   hl, 1
                ld   de, 0
                ld   bc, #200
                ldir
                ; halt end

                ld   BC, (BCRegSave)
                dec  bc
                ld   a, b
                or   c
                jr   nz, cycle
                ;ei
                ;ret
endProgram      ld   sp, (stackSave)

                ; Dates(born and died) will be pixeled here
                ld   hl, #0112
                ld   (PixelCounter), hl
                ld   hl, DatesPixels
nextPlot        ld   c, (hl)
                inc  hl
                ld   b, (hl)
                inc  hl
                push hl
                push af
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
                
                ; pause
                ld   hl, 1
                ld   de, 0
                ld   bc, #900
                ldir
                
                pop  af
                pop  hl
                ld   de, (PixelCounter)
                dec  de
                ld   a, d
                or   e
                jr   z, EndPixelPrint
                ld   (PixelCounter), de
                
                jr   nextPlot
EndPixelPrint:
                ei
                ret
PixelCounter:
DEFW 548

; screen libs
include "..\..\Libs\Constants.lib"
include "..\..\Libs\PixelToScrAdress.asm"

; random pixels on the screen(Cobain picture)
RandomPixels:
include "randoms.inc"

; Dates(born and died) pixels
DatesPixels:
incbin "dates_pixels.bin"

picCobain:
incbin "cobain.scr"

stackSave:
DEFW 0 ; here will be saved stack pointer

BCRegSave:
DEFW 0