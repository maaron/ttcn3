grammar TTCN3;

options {
  language=CSharp3;
  output=AST;
}

tokens {
  /* Imaginary tokens:  These are used to indicate various types of grammar 
     structures.  Generally used for nodes in the AST that contain children. */
  FIELDDEF; TYPEDEF; TYPEDEFS; MODULEDEF; IMPORTDEF; GROUPDEF; ENUMDEF; ALLOWEDVALUES;
}

/* Possible alternative to dealing with FLOAT/DOTDOT conflict
@lexer::header {
    using System.Collections.Generic;
}
 
@lexer::members {
    Queue<IToken> tokens = new Queue<IToken>();
    public override void Emit(IToken token) 
    {
        state.token = token;
        tokens.Enqueue(token);
    }
    public override IToken NextToken()
    {
        base.NextToken();
        if ( tokens.Count ==0 )
            return Token.EOF_TOKEN;
        return tokens.Dequeue();
    }
}
*/

@header {
using System;
}

@members {
    public static void PrintTree(ITree tree, string indent)
    {
      for (int i = 0; i < tree.ChildCount; i++)
      {
        Console.WriteLine("{0}{1}", indent, tree.GetChild(i));
        PrintTree(tree.GetChild(i), indent + "  ");
      }
    }
    
    public static void Main(string[] args) {
        var fileStream = new ANTLRFileStream(args[0]);
        var lexer = new TTCN3Lexer(fileStream);
        var parser = new TTCN3Parser(new CommonTokenStream(lexer));
        parser.TreeAdaptor = new CommonTreeAdaptor();

        /* Uncomment this to test the lexer */
        /*
        while (fileStream.LA(1) != CharStreamConstants.EndOfFile)
	{
	    var token = lexer.NextToken();
            if (token.Channel != 99)
            {
              Console.WriteLine("token {0}/{1}/{2}", token.Type, tokenNames[token.Type], token.Text);
            }
	}
        */
        
        try {
            var result = parser.moduleDef();
            Console.WriteLine("AST:");
            Console.WriteLine("{0}", ((CommonTree)result.Tree));
            PrintTree((CommonTree)result.Tree, " ");
 
        } catch (Exception e)  {
            Console.WriteLine("Parsing exception: {0}", e.Message);
            Console.Error.WriteLine(e.StackTrace);
        }
    }
}

typeDefs:	typeDef* -> ^(TYPEDEFS typeDef*)
	;

moduleDef
	:	MODULE ID LBRACKET moduleDefinitionsList? RBRACKET SEMICOLON? -> ^(MODULEDEF ID moduleDefinitionsList?)
	;

moduleDefinitionsList
	:	(moduleDefinition SEMICOLON?)+ -> moduleDefinition+
	;
	
moduleDefinition
	:	(visibility? (typeDef | importDef | constDef) ) | (PUBLIC? groupDef)
	;

constDef:	CONST type constList SEMICOLON
	;
	
constList
	:	singleConstDef (',' singleConstDef)*
	;
	
singleConstDef
	:	ID ASSIGNCHAR constantExpression
	;

constantExpression
	:	singleExpression
	;

groupDef:	GROUP ID LBRACKET moduleDefinitionsList? RBRACKET -> ^(GROUPDEF ID moduleDefinitionsList?)
	;

importDef
	:	IMPORT 'from' ID 'all'? SEMICOLON -> ^(IMPORTDEF ID 'all'?)
	;

visibility
	:	PUBLIC | PRIVATE | FRIEND
	;

typeDef	:	TYPE typeDefBody -> ^(TYPEDEF typeDefBody)
	;

typeDefBody
	:	structuredTypeDef | subTypeDef
	;

structuredTypeDef
	:	recordDef | unionDef | enumDef | portDef
	;

portDef	:	PORT portDefBody
	;

portDefBody
	:	ID portDefAttribs
	;
	
portDefAttribs
	:	messageAttribs
	;
	
messageAttribs
	:	MESSAGE LBRACKET ((messageList) SEMICOLON? )+ RBRACKET
	;
	
messageList
	:	direction allOrTypeList
	;
	
direction
	:	IN | OUT
	;
	
allOrTypeList
	:	ALL | typeList
	;

typeList:	type (',' type)*
	;
	subTypeDef
	:	type ID subTypeSpec?
	;
	
subTypeSpec
	:	(allowedValuesSpec stringLength?) | stringLength
	;
	
stringLength
	:	LENGTH LPAREN singleExpression (DOTDOT bound)? RPAREN
	;
	
allowedValuesSpec
	:	LPAREN rangeDef RPAREN -> ^(ALLOWEDVALUES rangeDef)
	;
	
rangeDef:	bound '..' bound -> bound bound
	;
	
bound	:	(BANG? singleExpression) | (MINUS? INFINITY)
	;

recordDef 
	:	RECORD structDefBody
	;

unionDef:	UNION structDefBody
	;

enumDef	:	ENUMERATED ID LBRACKET enumerationList RBRACKET -> ^(ENUMDEF ID enumerationList)
	;

enumerationList
	:	enumeration (',' enumeration)* -> enumeration+
	;
	
enumeration
	:	ID (LPAREN MINUS? NUMBER RPAREN)? -> ID (MINUS? NUMBER)?
	;
	
structDefBody
	:	ID LBRACKET (structFieldDef (',' structFieldDef)*)? RBRACKET -> ID structFieldDef+
	;

structFieldDef
	:	type ID subTypeSpec? OPTIONAL? -> ^(FIELDDEF type ID subTypeSpec? OPTIONAL?)
	;
	
type 	:	predefinedType | referencedType
	;

referencedType
	:	extendedIdentifier
	;
	
extendedIdentifier
	:	ID ('.' ID)*
	;

predefinedType
	:	BITSTRING | BOOLEAN | CHARSTRING | CHAR | INTEGER | OCTETSTRING
	;

/* Expressions */
singleExpression
	:	xOrExpression (OR xOrExpression)*
	;
	
xOrExpression
	:	andExpression (XOR andExpression)*
	;
	
andExpression
	:	notExpression (AND notExpression)*
	;
		
notExpression
	:	NOT? equalExpression
	;

equalExpression	  :  	relExpression ( equalOp relExpression )*;
relExpression	  :  	shiftExpression (relOp shiftExpression)?;
shiftExpression	  :  	bitOrExpression ( shiftOp bitOrExpression )*;
bitOrExpression	  :  	bitXorExpression ( 'or4b' bitXorExpression )*;
bitXorExpression	  :  	bitAndExpression ( 'xor4b' bitAndExpression )*;
bitAndExpression	  :  	bitNotExpression ( 'and4b' bitNotExpression )*;
bitNotExpression	  :	'not4b'? addExpression;
addExpression	  :  	mulExpression ( addOp mulExpression )*;
mulExpression	  :  	unaryExpression ( multiplyOp unaryExpression )*;
unaryExpression	  :  	unaryOp? primary;
primary	  :  	value | (LPAREN singleExpression RPAREN);
addOp	  :  	 '+' | '-' | stringOp;
multiplyOp	  :  	 '*' | '/' | 'mod' | 'rem';
unaryOp	  :  	 '+' | '-';
relOp	  :  	 '<' | '>' | '>=' | '<=';
equalOp	  :  	 '==' | '!=';
stringOp	  :  	 '&';
shiftOp	  :  	 '<<' | '>>' | '<@' | '@>';
value	:	predefinedValue | referencedValue;
predefinedValue
	:	NUMBER;
referencedValue
	:	extendedIdentifier;

/* Expression Terminals */
OR	:	'or';
XOR	:	'xor';
AND	:	'and';
NOT	:	'not';

MODULE	:	'module';
PUBLIC	:	'public';
PRIVATE	:	'private';
FRIEND	:	'friend';
IMPORT	:	'import';
GROUP	:	'group';
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
UNION	:	'union';
ENUMERATED
	:	'enumerated';
TYPE 	:	'type';
LBRACKET:	'{';
RBRACKET:	'}';
SEMICOLON
	:	';';
LPAREN	:	'(';
RPAREN	:	')';
MINUS	:	'-';
ASSIGNCHAR
	:	':=';
BANG	:	'!';
CONST	:	'const';
INFINITY:	'infinity';
DOTDOT	:	'..';
LENGTH	:	'length';
IN	:	'in';
OUT	:	'out';
ALL	:	'all';
PORT	:	'port';
MESSAGE	:	'message';

ID  :	('a'..'z'|'A'..'Z'|'_') ('a'..'z'|'A'..'Z'|'0'..'9'|'_')*
    ;

/*
INT :	DIGIT+
    ;
*/

NUMBER	:	(NON_ZERO_NUM NUM+) | '0';

fragment
NON_ZERO_NUM
	:	'1'..'9';

fragment
NUM	:	'0' | NON_ZERO_NUM;

fragment
BIN_DIGIT
	:	'0' | '1';

fragment
HEX_DIGIT : ('0'..'9'|'a'..'f'|'A'..'F') ;

/*
FLOAT
    :   d=INT r=DOTDOT
          {
          $d.setType(INT);
          emit($d);
          $r.setType(DOTDOT);
          emit($r);
          }
    |	('0'..'9')+ '.' ('0'..'9')* EXPONENT?
    |   '.' ('0'..'9')+ EXPONENT?
    |   ('0'..'9')+ EXPONENT
    ;
*/

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
ESC_QUOTE
    :   '"' '"'
    ;