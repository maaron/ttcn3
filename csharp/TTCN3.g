grammar TTCN3;

options
{
  language=CSharp3;
  output=AST;
}

@header {
using System;
}

@members {
    public static void Main(string[] args) {
        var fileStream = new ANTLRFileStream(args[0]);
        var lexer = new TTCN3Lexer(fileStream);
        var parser = new TTCN3Parser(new CommonTokenStream(lexer));
        
        /*
        try {
            while (fileStream.LA(1) != CharStreamConstants.EndOfFile)
            {
              var token = lexer.NextToken();
              Console.WriteLine("token: {0}", token);
            }
        } catch (Exception e)  {
            Console.Error.WriteLine(e.StackTrace);
        }
        */
 
        try {
            var result = parser.typeDef();
            Console.WriteLine("result {0}", result.Tree);
        } catch (Exception e)  {
            Console.Error.WriteLine(e.StackTrace);
        }
    }
}

typeDef	:	TYPE structuredTypeDef {Console.WriteLine("Found typeDef");}
	;

structuredTypeDef
	:	recordDef
	;

recordDef 
	:	RECORD structDefBody
	;

structDefBody
	:	ID LBRACKET (structFieldDef (',' structFieldDef)*)? RBRACKET
	;

structFieldDef
	:	type ID OPTIONAL?
	;
	
type 	:	predefinedType {Console.WriteLine("Found typedef start");}
	;

predefinedType
	:	BITSTRING | BOOLEAN | CHARSTRING | CHAR | INTEGER | OCTETSTRING
	;

OPTIONAL:	'optional';
BITSTRING
	:	'bitstring';
BOOLEAN	:	'boolean';
CHARSTRING
	:	'charstring';
CHAR	:	'char';
INTEGER	:	'integer';
OCTETSTRING
	:	'octetstring';
RECORD	:	'record';
TYPE 	:	'type';
LBRACKET:	'{';
RBRACKET:	'}';

ID  :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
    ;

INT :	'0'..'9'+
    ;

FLOAT
    :   ('0'..'9')+ '.' ('0'..'9')* EXPONENT?
    |   '.' ('0'..'9')+ EXPONENT?
    |   ('0'..'9')+ EXPONENT
    ;

COMMENT
    :   '//' ~('\n'|'\r')* '\r'? '\n' {$channel=Hidden;}
    |   '/*' ( options {greedy=false;} : . )* '*/' {$channel=Hidden;}
    ;

WS  :   ( ' '
        | '\t'
        | '\r'
        | '\n'
        ) {$channel=Hidden;}
    ;

STRING
    :  '"' ( ESC_QUOTE | ~('\\'|'"') )* '"'
    ;

fragment
EXPONENT : ('e'|'E') ('+'|'-')? ('0'..'9')+ ;

fragment
HEX_DIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;

fragment
ESC_QUOTE
    :   '"' '"'
    ;

fragment
OCTAL_ESC
    :   '\\' ('0'..'3') ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7') ('0'..'7')
    |   '\\' ('0'..'7')
    ;

fragment
UNICODE_ESC
    :   '\\' 'u' HEX_DIGIT HEX_DIGIT HEX_DIGIT HEX_DIGIT
    ;
