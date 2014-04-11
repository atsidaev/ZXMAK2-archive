;---------------------------------------------------------------;
; PixelToScrAdress                                              ;
;                                                               ;
;   Converts a screen pixel coord into a screen address and     ;
;   pixel position                                              ;
;   Written by Tony Thompson                                    ;
;   Created         1984                                        ;
;   Last Changed    1st May 2003                                ;
;                                                               ;
;   Inputs                                                      ;
;       b - y position in PIXELS                                ;
;       c - x position in PIXELS                                ;
;                                                               ;
;   Outputs                                                     ;
;       hl - the attribute address for the screen location      ;
;       a  - contains the bit position of the pixel             ;
;                                                               ;
;   Regs Used                                                   ;
;       af,  bc,  hl                                            ;
;                                                               ;
;   Regs destroyed                                              ;
;       af                                                      ;
;---------------------------------------------------------------;
PixelToScrAdress:
            ld a, b
            rra
            scf
            rra
            rra
            and 88
            ld h, a
            ld a, b
            and 7
            add a, h
            ld h, a
            ld a, c
            rrca
            rrca
            rrca
            and 31   ; = 11111 binary
            ld l, a
            ld a, b
            and 56
            add a, a
            add a, a
            or l
            ld l, a
            ld a, c
            and 7
            ret