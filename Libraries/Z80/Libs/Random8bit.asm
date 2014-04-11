; source: http://wikiti.brandonw.net/index.php?title=Z80_Routines:Math:Random
;
;
;
; -----> Generate a random number
; ouput a=answer 0<=a<=255
; all registers are preserved except: af
;

random:
        push    hl
        push    de
        ld      hl,(randData)
        ld      a,r
        ld      d,a
        ld      e,(hl)
        add     hl,de
        add     a,l
        xor     h
        ld      (randData),hl
        pop     de
        pop     hl
        ret

randData: DEFB 0x7F, 0xBF