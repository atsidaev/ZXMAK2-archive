;---------------------------------------------------------------;
; IncYCoor                                                      ;
;                                                               ;
;   Moves the screen address down 1 line                        ;
;                                                               ;
;   Written by Tony Thompson                                    ;
;   Written by Nick Fleming                                     ;
;   Both versions where identical                               ;
;   Created         1984                                        ;
;   Last Changed    1st May 2003                                ;
;                                                               ;
;   Inputs                                                      ;
;       HL - the address of a screen location                   ;
;                                                               ;
;   Outputs                                                     ;
;       HL - the address of the line below                      ;
;                                                               ;
;   Regs Used                                                   ;
;       AF,  HL                                                 ;
;                                                               ;
;   Regs destroyed                                              ;
;       AF                                                      ;
;---------------------------------------------------------------;
IncYCoor:  inc h                       ; try to move down 1 line in a character                1M    4T
           ld  a, h                    ; get h into a                                          1M    4T
           and 7                       ; test if still inside character                        2M    7T
           ret nz                      ; ret if in character square                            1M    5T
           ld  a, l                    ; no,  get lower byte of address                        1M    4T
           add a, 32                   ; and move it to the next character block               2M    7T
           ld  l, a                    ; store the result                                      1M    4T
           ret c                       ; return if we are still in the same segment            1M    5T
           ld  a, h                    ; no,  so need to adjust high order byte of address     1M    4T
           sub 8                       ; adjust screen segment                                 2M    7T
           ld  h, a                    ; store the correction                                  1M    4T
           ret                         ;                                                       1M   10T
                                       ;                                                      =========
                                       ;                                                      15M   65T
           
; Legend: - M = bytes in memory
;         - T = processor tacts
