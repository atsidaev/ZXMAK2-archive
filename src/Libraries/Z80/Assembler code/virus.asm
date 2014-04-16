;****************************************************************
;
; Description: First 8 pixels on the screen will be permanently 
;       changing while the speccy is running and accepting commands. 
;       Example how to implement background process on ZX Spectrum.
;
; Author: Adler, 2014
;
;****************************************************************

;* Main(do not forget CLEAR 29999)
        org #7530 ; = 30000
        ld a, #75
        ld i, a
        im 2      ; let the show starts
        ret 

; Interruption routine
        org #75FF
DEFB    #01, #76   ; Define IM 2 vector
        di
        push af
        ld a, r
        ld (16384), a
        pop af
        call #38
        reti