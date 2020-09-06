grammar KeywordTest;

procedureUnit
    :	keywordProcedureCall ';'
	|	variableDeclaration ';'
    |   statementExpression ';'
    ;

keywordProcedureCall : procedureAndArgumentCombinedPhrase '(' expressionList? ')' ;

procedureAndArgumentCombinedPhrase
    :   Identifier ('.' Identifier)+ (literal | (qualifiedName literal?)) ( qualifiedName literal? )*
    ;

variableDeclaration : type Identifier ('=' expression)? ;

type
    :   Identifier ('.' Identifier )*
    |   'bool'
    |   'int'
    ;

literal : IntegerLiteral | booleanLiteral ;
booleanLiteral : 'true' | 'false' ;

expressionList
    :   expression (',' expression)*
    ;

statementExpression
    :   expression
    ;

expression
    :   primary
    |   expression '.' Identifier
    |   expression '[' expressionList ']'
    |   expression '(' expressionList? ')'
    |   expression ( '++' | '--' )
    |   ( '+' | '-' | '++' | '--' ) expression
    |   ( '~' | '!' ) expression
    |   expression ( '*' | '/' | '%' ) expression
    |   expression ( '+' | '-' ) expression
    |   expression ( '<' '<' | '>' '>' ) expression
    |   expression ( '<' '=' | '>' '=' | '>' | '<' ) expression
    |   expression ( '==' | '!=' ) expression
    |   expression '&' expression
    |   expression '^' expression
    |   expression '|' expression
    |   expression '&&' expression
    |   expression '||' expression
    |   expression '?' expression ':' expression
    |   <assoc=right> expression
        ( '+='
        | '-='
        | '*='
        | '/='
        | '='
        )
        expression
    ;


primary
    :   '(' expression ')'
    |   literal
    |   Identifier
    ;

qualifiedName : Identifier ('.' Identifier)* ;

IntegerLiteral : ('0' | '1'..'9' '0'..'9'*) ;

Identifier : Letter (Letter | Digit)* ;

fragment Letter :  'a'..'z' | 'A'..'Z' ;
fragment Digit :  '0'..'9' ;

COMMENT
    :   '/*' .*? '*/'    -> channel(HIDDEN) // match anything between /* and */
    ;
WS  :   [ \r\t\u000C\n]+ -> channel(HIDDEN)
    ;

LINE_COMMENT
    : '//' ~[\r\n]* '\r'? '\n' -> channel(HIDDEN)
    ;
