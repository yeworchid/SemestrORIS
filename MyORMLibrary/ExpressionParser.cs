using System;
using System.Collections.Generic;
using System.Linq.Expressions;

// делаем класс internal чтобы он был доступен только внутри библиотеки
internal class ExpressionParser
{
    // парсинг Expression в SQL
    public static (string whereClause, Dictionary<string, object> parameters) Parse<T>(Expression<Func<T, bool>> predicate)
    {
        var parameters = new Dictionary<string, object>();
        int paramCounter = 0;
        
        string whereClause = ParseNode(predicate.Body, parameters, ref paramCounter);
        
        return (whereClause, parameters);
    }

    // рекурсивный парсинг узлов expression tree
    private static string ParseNode(Expression expression, Dictionary<string, object> parameters, ref int paramCounter)
    {
        // бинарные операции (==, !=, >, <, >=, <=, &&, ||)
        if (expression is BinaryExpression binaryExpr)
        {
            string left = ParseNode(binaryExpr.Left, parameters, ref paramCounter);
            string right = ParseNode(binaryExpr.Right, parameters, ref paramCounter);
            
            string op = binaryExpr.NodeType switch
            {
                ExpressionType.Equal => "=",
                ExpressionType.NotEqual => "!=",
                ExpressionType.GreaterThan => ">",
                ExpressionType.GreaterThanOrEqual => ">=",
                ExpressionType.LessThan => "<",
                ExpressionType.LessThanOrEqual => "<=",
                ExpressionType.AndAlso => "AND",
                ExpressionType.OrElse => "OR",
                _ => throw new NotSupportedException($"Операция {binaryExpr.NodeType} не поддерживается")
            };
            
            // для AND/OR добавляем скобки
            if (op == "AND" || op == "OR")
            {
                return $"({left} {op} {right})";
            }
            
            return $"{left} {op} {right}";
        }
        
        // доступ к свойству (например, x.Name)
        if (expression is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }
        
        // константа или значение
        if (expression is ConstantExpression constantExpr)
        {
            string paramName = $"@p{paramCounter++}";
            parameters[paramName] = constantExpr.Value;
            return paramName;
        }
        
        // унарные операции (например, !x.IsActive)
        if (expression is UnaryExpression unaryExpr)
        {
            if (unaryExpr.NodeType == ExpressionType.Not)
            {
                string operand = ParseNode(unaryExpr.Operand, parameters, ref paramCounter);
                return $"NOT {operand}";
            }
            
            // Convert операции просто пропускаем
            if (unaryExpr.NodeType == ExpressionType.Convert)
            {
                return ParseNode(unaryExpr.Operand, parameters, ref paramCounter);
            }
        }
        
        // вызов метода (например, x.Name.Contains("test"))
        if (expression is MethodCallExpression methodCallExpr)
        {
            // Contains
            if (methodCallExpr.Method.Name == "Contains")
            {
                string property = ParseNode(methodCallExpr.Object, parameters, ref paramCounter);
                string value = ParseNode(methodCallExpr.Arguments[0], parameters, ref paramCounter);
                
                // заменяем параметр на LIKE паттерн
                string paramName = value;
                if (parameters.ContainsKey(paramName))
                {
                    parameters[paramName] = $"%{parameters[paramName]}%";
                }
                
                return $"{property} LIKE {value}";
            }
            
            // StartsWith
            if (methodCallExpr.Method.Name == "StartsWith")
            {
                string property = ParseNode(methodCallExpr.Object, parameters, ref paramCounter);
                string value = ParseNode(methodCallExpr.Arguments[0], parameters, ref paramCounter);
                
                string paramName = value;
                if (parameters.ContainsKey(paramName))
                {
                    parameters[paramName] = $"{parameters[paramName]}%";
                }
                
                return $"{property} LIKE {value}";
            }
            
            // EndsWith
            if (methodCallExpr.Method.Name == "EndsWith")
            {
                string property = ParseNode(methodCallExpr.Object, parameters, ref paramCounter);
                string value = ParseNode(methodCallExpr.Arguments[0], parameters, ref paramCounter);
                
                string paramName = value;
                if (parameters.ContainsKey(paramName))
                {
                    parameters[paramName] = $"%{parameters[paramName]}";
                }
                
                return $"{property} LIKE {value}";
            }
        }
        
        throw new NotSupportedException($"Expression типа {expression.GetType().Name} не поддерживается");
    }
}
