parser grammar StepBro;

options { tokenVocab=StepBroLexer; }

@parser::header {#pragma warning disable 3021}

@parser::members 
{
    // Place helper code here
}

compilationUnit : fileProperties? usingDeclarations namespaceDeclaration? fileElements EOF ;

fileProperties : elementPropertyblock ;

usingDeclarations : usingDeclaration* ;

usingDeclaration 
    :   PUBLIC? USING identifierOrQualified SEMICOLON                   # UsingDeclarationWithIdentifier
    |   PUBLIC? USING typedefName ASSIGNMENT typedefType SEMICOLON      # TypeAlias
    |   PUBLIC? USING (REGULAR_STRING | VERBATIUM_STRING) SEMICOLON     # UsingDeclarationWithPath
    ;

namespace : identifierOrQualified ;

namespaceDeclaration : NAMESPACE namespace SEMICOLON ;

fileElements : fileElement* ;
fileElement 
    :	attributes? elementModifier?
        (	enumDeclaration 
        |	configDeclaration 
        |   fileVariable
        |   procedureOrFunction
        |	testlist
        |	datatable
        |   fileElementOverride
        |   typedef
        ) 
    ;

procedureOrFunction 
    :	FUNCTION procedureReturnType procedureName procedureParameters 
            (COLON elementPropertyList)?
            procedureBodyOrNothing                                                          # FileElementFunction 
    |	PROCEDURE? procedureReturnType procedureName procedureParameters 
            (COLON elementPropertyList)?
            procedureBodyOrNothing                                                          # FileElementProcedure 
    ;

procedureDeclaration	// For testing
    :   attributes? 
        (PROCEDURE | FUNCTION)? 
        procedureReturnType 
        procedureName 
        procedureParameters 
        (COLON elementPropertyList)? 
        procedureBodyOrNothing
    ;

procedureParameters : formalParameters ;
procedureReturnType : type ;
procedureName : IDENTIFIER ;
//baseName : identifierOrQualified ;
procedureBody : block ;
procedureBodyOrNothing : procedureBody | SEMICOLON ;

elementModifier
    :   PUBLIC			// Shared with all
    |   PRIVATE			// Shared within the file
    |	PROTECTED		// Shared within the namespace
    ;

overrideReference : identifierOrQualified ;

fileElementOverride : OVERRIDE overrideReference typeOverride? (elementPropertyblock | SEMICOLON) ;

typeOverride : AS typedefName ;

typedef 
    : TYPEDEF typedefName COLON typedefType SEMICOLON       // NOT LIKE A TYPEDEF IN C/C++; NAME FIRST HERE!!
    ;

typedefName : IDENTIFIER ;

typedefType : typeSimpleOrGeneric ;

fileVariable
    :   elementModifier? variableType variableDeclaratorId ASSIGNMENT ctorClassType ctorArguments? elementPropertyblock     #FileVariableWithPropertyBlock
    |   elementModifier? variableType variableDeclarator SEMICOLON                                                          #FileVariableSimple
    ;

ctorClassType : type ;

ctorArguments : arguments ;
 
//modifiers
    //:   modifier*
    //;

// ENUM DECLARATION

enumDeclaration : ENUM IDENTIFIER (COLON enumParent)? enumBody ;
enumParent : identifierOrQualified ;
enumBody : OPEN_BRACE enumValues? CLOSE_BRACE ;
enumValues : enumValue (COMMA enumValue)* COMMA? ;
enumValue : IDENTIFIER (ASSIGNMENT (INTEGER_LITERAL | HexLiteral)) ? ;

// CONFIGURATION DATA
configDeclaration : CONFIG IDENTIFIER elementPropertyblock ;

// TESTLIST

testlist : 
    TESTLIST
    testListName 
    arguments? 
    (COLON elementPropertyList)? 
    (testlistBlock | SEMICOLON)
    ;

testListName : IDENTIFIER ;

testListParent : identifierOrQualified ;

testlistBlock :  OPEN_BRACE testListEntry* CLOSE_BRACE ;

testListEntry : STAR identifierOrQualified testListEntryArguments? ;

testListEntryArguments : arguments ;

        //testlist TestAllKindOfNumbers(timespan maxtime) : StandardTestList, AddressSetup: Danish, Timeout: 10s
        //{
        //	* TestEvenNumbers( looptime: maxtime )
        //	* TestUnevenNumbers
        //}

// DATATABLE

datatable : DATATABLE datatableName (COLON elementPropertyList)? (datatableRows | SEMICOLON) ;
datatableOnly : datatable EOF ;

datatableName : IDENTIFIER ;

datatableRows : datatableRow datatableRow* ;
datatableRow  : DATATABLE_ROW_START (datatableRowCell DATATABLE_CELL_DELIMITER)+ (DATATABLE_END_LINE | DATATABLE_END_LINE_COMMENT) ;
datatableRowCell : DATATABLE_CELL_CONTENT ;

datatableRowCellContent
    :   literal
    |   identifierOrQualified
    ;

// VARIABLES

//constantDeclarator
//    :   Identifier constantDeclaratorRest
//    ;
//constantDeclaratorsRest : constantDeclaratorRest (COMMA constantDeclarator)* ;
//constantDeclaratorRest : (OPEN_BRACKET CLOSE_BRACKET)* ASSIGNMENT variableInitializer ;


//fileVariableDeclarationStatement
//    :    localVariableDeclaration SEMICOLON
//    ;

//localVariableDeclarationStatement
//    :    localVariableDeclaration SEMICOLON
//    ;

calculatorExpression 
    : VAR variableDeclaratorId ASSIGNMENT variableInitializer   # CalcVarExpression
    | expression                                                # CalcExpression
    ;

localVariableDeclaration	// Used in procedure-scope and in for-loop initializer.
    :   variableModifier? variableType variableDeclarators
    ;

simpleVariableDeclaration : variableType variableDeclaratorWithAssignment ;	// One single variable; used in using statement.

variableType : variableVarType | type ;
variableVarType : VAR ;

//variableModifiers : variableModifier* ;

variableModifier
    :   STATIC				// 
    |	EXECUTION			//
    |   SESSION				//
    ;

variableDeclarators
    :   variableDeclaratorlist								# SimpleVarDeclarators
    //|	(arguments)? Identifier elementPropertyblock?		# DirectObjectInitializer
    ;

variableDeclaratorlist : variableDeclarator (COMMA variableDeclarator)* ;

variableDeclarator : variableDeclaratorWithAssignment | variableDeclaratorWithoutAssignment ;

variableDeclaratorWithAssignment : variableDeclaratorId ASSIGNMENT variableInitializer ;
variableDeclaratorWithoutAssignment : variableDeclaratorId ;
//variableDeclaratorWithPropertyBlock : variableDeclaratorId ;
//variableDeclaratorWithoutAssignment : variableDeclaratorId elementPropertyblock;

variableDeclaratorId : IDENTIFIER ;
variableDeclaratorQualifiedId : identifierOrQualified ;

variableInitializer
    :   arrayInitializer		# VariableInitializerArray
    |   primaryExpression		# VariableInitializerExpression
    ;

arrayInitializer : OPEN_BRACE (variableInitializer (COMMA variableInitializer)* (COMMA)? )? CLOSE_BRACE ;

//modifier
    //:   PUBLIC
    //|   PRIVATE
    //|   STATIC
    //|   'protected'
    //|   'abstract'
    //|   'final'
    //|   'native'
    //|   'synchronized'
    //|   'transient'
    //|   'volatile'
    //|   'strictfp'
    //;

//packageOrTypeName
//    :   identifierOrQualified
//    ;

enumConstantName
    :   IDENTIFIER
    ;

type
    :	VOID								# TypeVoid
    |	primitiveType typeBrackets			# TypePrimitive
    |   identifierOrQualified typeBrackets	# TypeClassOrInterface
    |	PROCEDURE							# TypeProcedure
    |	FUNCTION							# TypeFunction
    |	TESTLIST							# TypeTestList
    ;

typeBrackets : (OPEN_BRACKET CLOSE_BRACKET)? (OPEN_BRACKET CLOSE_BRACKET)? (OPEN_BRACKET CLOSE_BRACKET)? ;

primitiveType
    :   BOOL
    |   INT_
    |   INTEGER
    |   DECIMAL
    |	DOUBLE
    |	VERDICT
    |   DATETIME
    |   TIMESPAN
    |   STRING
    |	OBJECT
    ;

typeReference : TYPEOF OPEN_PARENS type CLOSE_PARENS ;

typeSimpleOrGeneric
    : type typeParameters       # TypeGeneric
    | type                      # TypeSimple
    ;

typeParameters : LT typeParameter (COMMA typeParameter)* GT ;

typeParameter : typeSimpleOrGeneric ;

//qualifiedNameList
//    :   identifierOrQualified (COMMA identifierOrQualified)*
//    ;

formalParameters
    :   OPEN_PARENS formalParameterDecls? CLOSE_PARENS
    ;

formalParameterDecls
    :   formalParameterDecl (COMMA formalParameterDecl)*
    ;

formalParameterDecl
    :   formalParameterDeclStart (ASSIGNMENT formalParameterAssignment)?
    ;

formalParameterDeclStart
    :   formalParameterModifiers? formalParameterType IDENTIFIER
    ;

formalParameterType : type ;

formalParameterModifiers : THIS ;

formalParameterAssignment : expression ;

//formalParameterDecls
//    :   variableModifiers type formalParameterDeclsRest
//    ;

//formalParameterDeclsRest
//    :   variableDeclaratorId (COMMA formalParameterDecls)?
//    //|   '...' variableDeclaratorId
//    ;

//constructorBody
//    :   '{' explicitConstructorInvocation? blockStatement* '}'
//    ;

//explicitConstructorInvocation
//    :   nonWildcardTypeArguments? ('this' | 'super') arguments ';'
//    |   primary '.' nonWildcardTypeArguments? 'super' arguments ';'
//    ;

literal
    :   integerLiteral			# LiteralInt
    |   FloatingPointLiteral	# LiteralFloat
    //|   CharacterLiteral
    |   stringLiteral			# LiteralString
    |   IDENTIFIER_LITERAL      # LiteralIdentifier
    |   booleanLiteral			# LiteralBool
    |	verdictLiteral			# LiteralVerdict
    |   NULL					# LiteralNull
    |	TimespanLiteral			# LiteralTimespan
    |	DateTimeLiteral			# LiteralDateTime
    |	RangeLiteral			# LiteralRange
    |	BinaryBlockLiteral		# LiteralBinaryBlock
    ;

integerLiteral
    :   HexLiteral					# LiteralHex
    //|   OctalLiteral				# LiteralOctal
    |   INTEGER_LITERAL				# LiteralInteger
    |   IntegerWithSIPrefixLiteral	# LiteralInteger
    ;

booleanLiteral
    :   TRUE
    |   FALSE
    ;

verdictLiteral
    :   UNSET
    |   PASS
    |	FAIL
    |	INCONCLUSIVE
    |	ERROR
    ;

stringLiteral
    : interpolated_regular_string
    | interpolated_verbatium_string
    | REGULAR_STRING
    | VERBATIUM_STRING
    ;

interpolated_regular_string
    : INTERPOLATED_REGULAR_STRING_START interpolated_regular_string_part* DOUBLE_QUOTE_INSIDE
    ;


interpolated_verbatium_string
    : INTERPOLATED_VERBATIUM_STRING_START interpolated_verbatium_string_part* DOUBLE_QUOTE_INSIDE
    ;

interpolated_regular_string_part
    : interpolated_string_expression
    | DOUBLE_CURLY_INSIDE
    | REGULAR_CHAR_INSIDE
    | REGULAR_STRING_INSIDE
    ;

interpolated_verbatium_string_part
    : interpolated_string_expression
    | DOUBLE_CURLY_INSIDE
    | VERBATIUM_DOUBLE_QUOTE_INSIDE
    | VERBATIUM_INSIDE_STRING
    ;

interpolated_string_expression
    : expression (',' expression)* (':' FORMAT_STRING+)?
    ;

// STATEMENTS / BLOCKS

block : OPEN_BRACE blockStatements CLOSE_BRACE ;

blockStatements : blockStatement* ;
blockStatement : blockStatementAttributes? block | statement ;
blockStatementAttributes : attributes ;

subStatement : block | statement ;

statement
    :   (ASSERT | EXPECT) (REGULAR_STRING COLON)? parExpression SEMICOLON			            # expectStatement
    |   IF parExpression subStatement (ELSE subStatement)?							            # ifStatement
    |   FOR OPEN_PARENS forControl CLOSE_PARENS ( COLON statementPropertyList )? subStatement   # forStatement
    |   FOREACH OPEN_PARENS foreachControl CLOSE_PARENS subStatement				            # foreachStatement
    |   WHILE parExpression ( COLON statementPropertyList )? subStatement	                    # whileStatement
    |   DO subStatement WHILE parExpression SEMICOLON								            # doWhileStatement
    |	USING OPEN_PARENS usingExpression CLOSE_PARENS subStatement					            # usingStatement
    //|   'try' block
    //    ( catches 'finally' block
    //    | catches
    //    | 'finally' block
    //    )
    //|   'switch' parExpression switchBlock
    |   ALT altBlock																# altStatement
    |   INTERLEAVE interleaveBlock													# interleaveStatement
    |   RETURN expression? SEMICOLON												# returnStatement
    |   THROW expression SEMICOLON													# throwStatement
    |   STEP ((stepIndex COMMA stepTitle) | stepIndex | stepTitle)? SEMICOLON		# stepStatement
    |   LOG logModifier? parExpression SEMICOLON									# logStatement
    //|   REPORT parExpression SEMICOLON                                            # reportStatement
    |   BREAK SEMICOLON																# breakStatement
    |   CONTINUE SEMICOLON															# continueStatement
    |   GOTO IDENTIFIER SEMICOLON													# gotoStatement
    |   SEMICOLON																	# emptyStatement
    |   localVariableDeclaration SEMICOLON 											# localVariableDeclarationStatement
    |   keywordProcedureCall SEMICOLON												# keywordProcedureCallStatement
    |   callAssignment? 
            callReference 
            statementarguments 
            (( COLON statementPropertyblock ) | SEMICOLON)                          # callStatement
    |   expression SEMICOLON														# expressionStatement
    |   IDENTIFIER COLON															# labelStatement
    ;

expressionSt : expression SEMICOLON ;   // For testing expressions.

usingExpression : (simpleVariableDeclaration | expression) ;
stepIndex : INTEGER_LITERAL ;
stepTitle : REGULAR_STRING ;

keywordProcedureCall
    :	callAssignment? procedureAndArgumentCombinedPhrase statementarguments ( COLON statementPropertyblock )?
    ;

// Replaced with the 'procedureCallStatement':
//procedureCall : procedureReference statementarguments ( COLON elementpropertyblock )? ;

callAssignment : START | AWAIT | (identifierOrQualified ASSIGNMENT AWAIT?) ;
//procedureCallVariableCreation : variableModifier? variableType identifierOrQualified ASSIGNMENT ;

callReference :  identifierOrQualified  ;

procedureAndArgumentCombinedPhrase
    :   qualifiedName procedureAndArgumentCombinedPhraseTail
    //:   (Identifier '.')+ Identifier procedureAndArgumentCombinedPhraseTail
    //:   Identifier ('.' Identifier)+ ((qualifiedName literal?) | literal) ( qualifiedName literal? )*
    ;

procedureAndArgumentCombinedPhraseTail : procedureAndArgumentCombinedPhraseTailElement+ ;

procedureAndArgumentCombinedPhraseTailElement : keyword | identifierOrQualified | literal ;

logModifier
    :   WARNING
    |   ERROR
    ;

keywordType : CONFIG | OBJECT | OVERRIDE | PRIVATE | PUBLIC | VERDICT ;

keyword
    : BREAK | CONTINUE | DO | ELSE | ERROR | EXECUTION | EXPECT | FAIL
    | FOR | FUNCTION | IF | IGNORE | IN | INTERLEAVE | IS | LOG | NOT | ON | OUT
    | PASS | PROCEDURE | REF | RETURN // | REPORT 
    | SESSION | STEP | THIS | TIMEOUT | THROW | WARNING //| WITH
    ;

keywordWide : keywordType | keyword ;

//catches
//    :   catchClause (catchClause)*
//    ;

//catchClause
//    :   'catch' '(' formalParameter ')' block
//    ;

//formalParameter : type variableDeclaratorId ;

//formalParameter
//    :   variableModifiers type variableDeclaratorId
//    ;

//switchBlock
//    :   '{' switchBlockStatementGroup* switchLabel* '}'
//    ;

//switchBlockStatementGroup
//    :   switchLabel+ blockStatement*
//    ;

//switchLabel
//    :   'case' constantExpression ':'
//    |   'case' enumConstantName ':'
//    |   'default' ':'
//    ;

forControl
    :   forInit? SEMICOLON forCondition? SEMICOLON forUpdate?
    ;

forVariableDeclaration : variableType variableDeclarators ;

forInitExpression : expression ;

forInit : (forVariableDeclaration | forInitExpression) (COMMA forVariableDeclaration | forInitExpression)* ;

forCondition : expression ;

forUpdate : expressionList ;

foreachControl : variableType IDENTIFIER IN expression ;

// ALT and INTERLEAVE

altBlock
    :   OPEN_BRACE altBlockStatementGroup* CLOSE_BRACE
    ;

altBlockStatementGroup
    :   ( altEvent | ( OPEN_PARENS altEvent (OP_OR altEvent)* CLOSE_PARENS ) ) statement			// One or more events for each statement.
    ;

interleaveBlock
    :   OPEN_BRACE interleaveBlockStatementGroup* CLOSE_BRACE
    ;

interleaveBlockStatementGroup
    :   altEvent statement			// Only one event per statement
    ;

altEvent
    :   ON TIMEOUT ( TimespanLiteral | parExpression )			# AltEventTimeout		// NOT used when using 'else'.
    |   ON altEventGuard? identifierOrQualified arguments		# AltEventNormal
    |	ELSE													# AltEventElse			// NOT used when using timeout. 'else' is used when no waiting time is allowed.
    ;

altEventGuard : parExpression OP_AND ;


// PROPERTYBLOCK

elementPropertyblock : propertyblock ;
elementPropertyList : propertyblockStatementList ;

statementPropertyblock : propertyblock ;
statementPropertyList : propertyblockStatementList ;

propertyblock :	OPEN_BRACE propertyblockStatementList? propertyBlockCommaTooMuch? CLOSE_BRACE ;

propertyBlockCommaTooMuch : COMMA ;

propertyblockStatementList : propertyblockStatement ((COMMA propertyblockStatement) | propertyblockStatementMissingCommaSeparation)* ;
    
propertyblockStatement
    :	propertyblockStatementEvent
    |	propertyblockStatementNamed
    |	propertyblockStatementValueIdentifierOnly						// Short form of 'Identifier = true' or '<some property> = Identifier'
    ;

propertyblockStatementMissingCommaSeparation : propertyblockStatement ;

propertyblockStatementNamed 
    :	(   (propertyblockStatementTypeSpecifier propertyblockStatementTypeSpecifier propertyblockStatementTypeSpecifier propertyblockStatementNameSpecifier) | 
            (propertyblockStatementTypeSpecifier propertyblockStatementTypeSpecifier propertyblockStatementNameSpecifier) | 
            (propertyblockStatementTypeSpecifier propertyblockStatementNameSpecifier) | 
            propertyblockStatementNameSpecifier)
        op=(COLON | ASSIGNMENT | OP_ADD_ASSIGNMENT) 
        propertyblockStatementNamedValue ;

propertyblockStatementNameSpecifier : identifierOrQualified | primitiveType | keywordWide | REGULAR_STRING ;

propertyblockStatementTypeSpecifier : identifierOrQualified | primitiveType | keywordWide ;

propertyblockStatementNamedValue
    :	propertyblock
    |	propertyblockArray
    |	propertyblockStatementValueNormal
    ;

propertyblockStatementEvent 
    :	ON identifierOrQualified COLON verdictLiteral								# propertyblockEventVerdict
    |	ON identifierOrQualified COLON identifierOrQualified ASSIGNMENT expression	# propertyblockEventAssignment
    ;

propertyblockArray : OPEN_BRACKET propertyblockArrayEntryList? propertyBlockCommaTooMuch? CLOSE_BRACKET ;

propertyblockArrayEntryList : propertyblockArrayEntry ((COMMA propertyblockArrayEntry) | propertyblockArrayEntryMissingCommaSeparation)* ;

propertyblockArrayEntry
    :	propertyblock			# propertyblockArrayEntryPropertyBlock
    |	propertyblockArray		# propertyblockArrayEntryArray
    |	primaryOrQualified		# propertyblockArrayEntryPrimary
    ;

propertyblockArrayEntryMissingCommaSeparation : propertyblockArrayEntry ;

propertyblockStatementValueNormal : primaryOrQualified ;

propertyblockStatementValueIdentifierOnly 
    :	(propertyblockStatementTypeSpecifier IDENTIFIER)
    |   IDENTIFIER
    ;


// ATTRIBUTES
attributes
    : attribute_section+
    ;

attribute_section
    : OPEN_BRACKET propertyblockStatementList CLOSE_BRACKET
    ;

// LAMBDA EXPRESSION

lambdaParameters
    :	IDENTIFIER														# LambdaSimpleParameter
    |	OPEN_PARENS IDENTIFIER (COMMA IDENTIFIER)* CLOSE_PARENS			# LambdaMoreParameters
    |	OPEN_PARENS formalParameterDecls CLOSE_PARENS					# LambdaTypedParameters
    |	OPEN_PARENS CLOSE_PARENS										# LambdaNoParameters
    ;

lambdaExpression
    : lambdaParameters LAMBDA expression
    ;

// EXPRESSIONS

parExpression : OPEN_PARENS primaryExpression CLOSE_PARENS ;

statementarguments : arguments ;

arguments : OPEN_PARENS argumentList? CLOSE_PARENS ;

argumentList : argument (COMMA argument)* ;

argument : argumentName? (referenceArgument | primaryExpression) ;

argumentName : IDENTIFIER COLON ;

referenceArgument : ( OUT | REF ) IDENTIFIER ;

expressionList : expression (COMMA expression)* ;

constantExpression : expression ;	// Evaluation in code checks if expression is constant.

methodArguments : arguments ;

primaryExpression 
    :	lambdaExpression
    |	expression
    ;

expression
    :   primary															# ExpPrimary
    |   expression DOT IDENTIFIER                                       # ExpDotIdentifier
    |   expression methodArguments                                      # ExpParens
    |   expression OPEN_BRACKET argumentList CLOSE_BRACKET				# ExpBracket
    //|   expression '.' explicitGenericInvocation
    |   OPEN_BRACKET argumentList CLOSE_BRACKET							# ExpArray
    |   expression ( op=OP_INC | op=OP_DEC )							# ExpUnaryRight
    |   ( op=MINUS | op=OP_INC | op=OP_DEC ) expression					# ExpUnaryLeft
    |   ( op=TILDE | op=BANG | op=NOT ) expression						# ExpUnaryRight
    |   OPEN_PARENS type CLOSE_PARENS expression						# ExpCast
    |   AWAIT expression                                                # ExpAwait
    |   expression AS type                                              # ExpCastAs

    //|   NEW creator

    |	expression 
        (op1=OP_LE|op1=OP_LT_APPROX|op1=LT) 
        expression 
        (op2=OP_LE|op2=OP_LT_APPROX|op2=LT) 
        expression														# ExpBetween

    |	expression 
        ( op=OP_EQ | op=OP_EQ_APPROX ) 
        expression 
        TOLERANCE 
        expression														# ExpEqualsWithTolerance

    |   expression ( op=STAR | op=DIV | op=PERCENT ) expression			# ExpBinary
    |   expression ( op=PLUS | op=MINUS ) expression					# ExpBinary
    |   expression ( op=OP_LEFT_SHIFT | op=OP_RIGHT_SHIFT ) expression	# ExpBinary
    |   expression ( op=OP_LE | op=OP_GE | op=GT | op=LT ) expression	# ExpBinary
    |   expression ( op=OP_LT_APPROX | op=OP_GT_APPROX ) expression		# ExpBinary
    |   expression op=IS NOT? type										# ExpIsType
    |   expression ( op=OP_EQ | op=OP_NE | op=OP_EQ_APPROX ) expression	# ExpBinary
    |   expression op=AMP expression									# ExpBinary
    |   expression op=CARET expression									# ExpBinary
    |   expression op=BITWISE_OR expression								# ExpBinary
    |   expression op=OP_AND expression									# ExpBinary
    |   expression op=OP_OR expression									# ExpBinary
    |   expression OP_COALESCING expression								# ExpCoalescing
    |   expression INTERR expression COLON expression					# ExpSelect
    |   <assoc=right> expression
        ( op=OP_XOR_ASSIGNMENT
        | op=OP_ADD_ASSIGNMENT
        | op=OP_SUB_ASSIGNMENT
        | op=OP_MULT_ASSIGNMENT
        | op=OP_DIV_ASSIGNMENT
        | op=OP_AND_ASSIGNMENT
        | op=OP_OR_ASSIGNMENT
        | op=ASSIGNMENT
        | op=OP_RIGHT_SHIFT_ASSIGNMENT
        | op=OP_LEFT_SHIFT_ASSIGNMENT
        | op=OP_MOD_ASSIGNMENT
        )
        expression														# ExpAssignment
    ;


primary
    :   parExpression		//# PrimaryExp
    |   THIS				//# PrimaryThis
    |   literal				//# PrimaryLiteral
    |	IDENTIFIER
    |   typeReference
    ;

primaryOrQualified : primary | qualifiedName ;

identifierOrQualified : IDENTIFIER | qualifiedName ;

//creator
//    :   nonWildcardTypeArguments createdName classCreatorRest
//    |   createdName (arrayCreatorRest | classCreatorRest)
//    ;

//createdName
//    //:   classOrInterfaceType
//    :   primitiveType
//    ;

//innerCreator
//    :   nonWildcardTypeArguments? Identifier classCreatorRest
//    ;

//explicitGenericInvocation
//    :   nonWildcardTypeArguments Identifier arguments
//    ;

//arrayCreatorRest
//    :   '['
//        (   ']' ('[' ']')* arrayInitializer
//        |   expression ']' ('[' expression ']')* ('[' ']')*
//        )
//    ;

//classCreatorRest
//    :   arguments classBody?
//    ;

qualifiedName : IDENTIFIER (DOT IDENTIFIER)+ ;

//typeList
//    :   type (COMMA type)*
//    ;

//nonWildcardTypeArguments : LT type (COMMA type)* GT ;
