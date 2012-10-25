grammar Test; 

options 
{language = 'CSharp2'; 
output=AST; 
} 
expr : mexpr (PLUS^ mexpr)* SEMI! 
; 
mexpr 
: atom (STAR^ atom)* 
; 
atom: INT 
; 
//class csharpTestLexer extends Lexer; 
WS : (' ' 
| '\t' 
| '\n' 
| '\r') 
{ $channel = HIDDEN; } 
; 
LPAREN: '(' 
; 
RPAREN: ')' 
; 
STAR: '*' 
; 
PLUS: '+' 
; 
SEMI: ';' 
; 
protected 
DIGIT 
: '0'..'9' 
; 
INT : (DIGIT)+ 
;

