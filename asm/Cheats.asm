 ; Bomberman Jetters 8x8 Cheats Normmatt

.gba				; Set the architecture to GBA
.open "rom/output.gba",0x08000000		; Open input.gba for output.
					; 0x08000000 will be used as the
					; header size
					
.macro adr,destReg,Address
here:
	.if (here & 2) != 0
		add destReg, r15, (Address-here)-2
	.else
		add destReg, r15, (Address-here)
	.endif
.endmacro

.thumb
.org 0x0802AF60
; LDRB    R1, [R2,#6]
; ADDS    R0, R0, R1
; STRB    R0, [R2,#6]
LDRB    R1, [R2,#5] ; get maximum health
MOV		R0, R1
STRB    R0, [R2,#6] ; set current health to maximum ALWAYS


.pool
.close

 ; make sure to leave an empty line at the end
