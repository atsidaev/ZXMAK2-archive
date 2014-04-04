         org  30000
start    ld   c, #00 ;; X-coor, 0 at begin                  7T,  2M
loop     push bc     ;; save X-coor to C registry           11T, 1M
         ld   a,c    ;; working with X-coor                 4T,  1M
         add  a,a    ;; X*2 => 2 periods will be drawn.     4T,  1M
                     ;; If ommited => only 1 period will be drawn

sinus    add a,#40   ;; Start sin computing. Input: A=angle <0..255>
         ld  d,#00   ;; Routine will divide the circle to 256 parts
         ld  c,a
         add a,a
         add a,a
         ld  e,a
         sbc a,a
         xor e
         ld  e,a
         ld  h,d
         ld  l,e
         rra
mksin1   add hl,de
         dec a
         jr  nz,mksin1
         ld  a,#40
         add a,c
         add a,a
         sbc a,a
         xor h        ;; Sin computing finish
                      ;; Output: A=sin value in interval <0, 255> => means <-1; +1>

         pop bc       ;; restore X-coor from C registry
         rra          ;; Sin value divide by 2 => interval <0, 127>
         ld   b,a     ;; and it is Y-coor
         push bc
         call #22E5   ;; PLOT for coordinates X=C, Y=B
         pop  bc
         inc  c       ;; Move to next X-coor
         jr   nz,loop ;; and loop 256 pixels(speccy screen width)
         ret
         
; Legend: - M = bytes in memory
;         - T = processor tacts