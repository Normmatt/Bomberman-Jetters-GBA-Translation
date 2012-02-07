 ; Bomberman Jetters 8x8 font hack by Normmatt and Spikeman

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
.org 0x0800038A ; replace first set scene call to trigger debug mode
	BL 0x08003840 ;Original game code
	mov lr, pc ;need to add 8+1 on later
	ldr r0, =CheckKeys+1  ; my notes say this is free
	bx r0
.pool

.org 0x08453560 ; should be free for 0x100 bytes
.align 4
SetSceneFlag:
	LDR     R2, =0x300492C
	STR     R0, [R2]
	LDR     R2, =0x30022C0
	ADD     R0, R0, R2
	STRB    R1, [R0]
	
	mov		r0, #9
	add		lr, r0
	BX      LR
.pool

CheckKeys:
	ldr r0, =0x4000130
	ldrh r0, [r0]
	mov r1, #0xFC ;A+B+L+R
	cmp r0, r1
	beq DebugScene;
	
OriginalScene:
	mov r0, #0x3C
	b SetScene

DebugScene:
	mov r0, #0x5A

SetScene:
	mov r1, #1
	B SetSceneFlag
	
.pool
.close

 ; make sure to leave an empty line at the end
