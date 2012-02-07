 ; Bomberman Jetters 8x8 VWF for Item Names by Normmatt and Spikeman

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

.org 0x08015D16 ; routine that puts a character to the map
.area 0x08015D2A - 0x08015D16
	mov r3, #79
	add r3,r15
	mov r14, r3
    ldr r3, =putChar+1    ; r2 is best variable to use for jump
    bx r3
.pool
.endarea

.definelabel loadCharacter,0x08453660

.org 0x08454500 ; should be free space to put code
; r0 - character
; r1 - character color
; r2 - bg color
; r4 - vram location
putChar:
	;LDRB    R2, [R3,#0x12]
	;LDRB    R3, [R3,#0x13]

	PUSH    {R4-R7,LR}
	ADD  	SP,#-0x20
	
	mov     r6, r0 ;store character for a second
	mov     r7, r2 ;store bg color
	mov		r8, r5
	
	MOV     R3, SP
	BL      loadCharacter
	
	;mov r2, #5	; r2 = width, replace this with lookup table using r0
	ldr r2, =WidthTable
	ldrb r2, [r2,r6]
	
	bl GetNextTileAddress
	mov r3, r0
	
	ldr r6, [overflow]
	ldrb r1, [r6]
	mov r5, r1	; r5 = current overflow
	add r1, r1, r2	; r1 will be new overflow, r2 is spare after this
	cmp r1, #8
	ble NoNewTile	; if overflow >8 move to next tile
    mov r2, #8
	sub r1, r1, r2
	; clear next tile
	ldr r2, [mask]
	mul r2, r7 ; times 0x11111111 by the bg color pallete index
	str r2, [r3, #0]
	str r2, [r3, #4]
	str r2, [r3, #8]
	str r2, [r3, #12]
	str r2, [r3, #16]
	str r2, [r3, #20]
	str r2, [r3, #24]
	str r2, [r3, #28]
	
	; original code to increment stuff
	mov 	r2, r10
	ldrh 	r0, [r2]
	ADD     R0, #1          ; increment map address
	strh 	r0, [r2]
	
	mov r0, r8
	add r0, #1 ; increment tile number
	mov r8, r0
	
	LDR     R0, [SP,#0x58]  ; load number of characters
	ADD 	R0, #1 			; increment number of characters
	STR     R0, [SP,#0x58]  ; store number of characters

NoNewTile:
	strb r1, [r6]

	lsl r5, r5, 2	; *4, for 4bpp
	mov r6, #0x20	; i feel like this code should be somewhere else
	sub r6, r6, r5	; r6 is to shift the existing background

	mov r0, sp
	;r0 = font, r3 = overflow tile, r4 = VRAM, r5 will be shift

	bl PrintHalfChar

UpdateMapTile:
	LDR     R3, =0x3007470
	;LDR     R2, [R3,#0x40]
	MOV		R1, R10
	LDRH	R2, [R1]
	LDR     R0, [R3,#0x74]
	LSL     R2, R2, #1
	ADD     R2, R2, R0
	LDR     R0, [R3,#0x78]
	LDRH    R1, [R0,#0x10]
	LSL     R1, R1, #0xC
	;LDR     R0, [R3,#0x44]
	MOV		R0, R8 ;TODO
	ORR     R1, R0
	STRH    R1, [R2]
	
	; r0 should return tile address
	;LDR     R0, [R3,#0x44]
	
putChar_Exit:
	ADD  	SP,#0x20
	POP     {R4-R7}
	MOV		R5, R8
	LDR     R0, =0x3007470
	MOV		R8, R0
	
	;Quick hack for end of item name
	ldrb r0, [r7]
	cmp r0, #0xFF ;End of line
	bne putChar_Exit2
	
	;If its the end of the item name the increment map once more
	mov 	r1, r10
	ldrh 	r0, [r1]
	ADD     R0, #1          ; increment map address
	strh 	r0, [r1]
	
	; then reset overflow
	mov r0, #0
	ldr r1, [overflow]
	str r0, [r1]

putChar_Exit2:	
	POP		{PC}
	
PrintHalfChar:
	mov r7, 0	; r7 = loop counter

PrintHalfChar_loop:
	ldr r1, [r0,r7] ; sp = character data
	ldr r2, [r4,r7]
	lsl r1,r5
	lsl r2,r6	; shift out part of background to be overwritten
	lsr r2,r6	
	orr r1,r2
	str r1, [r4,r7]

	ldr r1, [r0,r7] ; now do overflow tile
	ldr r2, [r3,r7]
	lsr r1,r6	; swap shifts (i think this will work)
	lsr r2,r5
	lsl r2,r5	
	orr r1,r2
	str r1, [r3,r7]

	add r7,r7,4	; each row = 4 bytes
	cmp r7, #0x20	; are 8 rows printed?
	bne PrintHalfChar_loop
	bx r14
	
GetNextTileAddress:
	push {r1}
	LDR     R1, =0x3007470
	mov 	r0, r8
	add 	r0, #1 ; increment tile number
	
GetNextTileAddress_skip:	
	LSL     R0, R0, #5
	LDR     R1, [R1,#0x70]
	ADD     R0, R0, R1
	pop {r1}
	bx lr

ResetOverflow:
	; reset overflow
	mov r0, #0
	ldr r1, [overflow]
	str r0, [r1]
	bx lr

    
.align 4
overflow:  .word 0x03000000  ; my notes say this is free
mask:      .word 0x11111111  ; mask
.pool

WidthTable:
.incbin asm/bin/smallWidthTable.bin

.close

 ; make sure to leave an empty line at the end
