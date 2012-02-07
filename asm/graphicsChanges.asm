 ; Bomberman Jetters GFX Changes by Normmatt

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

.org 0x0825FB80 ; replace pointer
	.word titleScr
	
.org 0x08440000 ; should be free space to put this shit
titleScr:
	.incbin asm/bin/animated-titlescreen.lz10

.close

 ; make sure to leave an empty line at the end
