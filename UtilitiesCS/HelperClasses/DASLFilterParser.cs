using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UtilitiesCS.ReusableTypeClasses;

namespace UtilitiesCS
{
    public class DASLFilterParser
    {
        public TreeNode<string> Parse(string daslFilter)
        {
            if (string.IsNullOrEmpty(daslFilter))
                throw new ArgumentException("DASL filter cannot be null or empty", nameof(daslFilter));

            return ParseExpression(daslFilter);
        }

        private TreeNode<string> ParseExpression(string expression)
        {
            expression = expression.Trim();

            // Handle parentheses
            if (expression.StartsWith("(") && expression.EndsWith(")"))
            {
                var node = new TreeNode<string>("()");
                expression = expression.Substring(1, expression.Length - 2).Trim();
                node.Children.Add(ParseExpression(expression));
                return node;
            }

            // Check for AND/OR operators outside of parentheses
            var andIndex = FindOperatorIndex(expression, "AND");
            if (andIndex != -1)
            {
                var left = expression.Substring(0, andIndex).Trim();
                var right = expression.Substring(andIndex + 3).Trim();
                var node = new TreeNode<string>("AND");
                node.Children.Add(ParseExpression(left));
                node.Children.Add(ParseExpression(right));
                return node;
            }

            var orIndex = FindOperatorIndex(expression, "OR");
            if (orIndex != -1)
            {
                var left = expression.Substring(0, orIndex).Trim();
                var right = expression.Substring(orIndex + 2).Trim();
                var node = new TreeNode<string>("OR");
                node.Children.Add(ParseExpression(left));
                node.Children.Add(ParseExpression(right));
                return node;
            }

            // Base case: no AND/OR operators, return a leaf node
            return new TreeNode<string>(expression);
        }

        private int FindOperatorIndex(string expression, string operatorStr)
        {
            int depth = 0;
            for (int i = 0; i < expression.Length; i++)
            {
                if (expression[i] == '(')
                {
                    depth++;
                }
                else if (expression[i] == ')')
                {
                    depth--;
                }
                else if (depth == 0)
                {
                    if (expression.Substring(i).StartsWith(operatorStr, StringComparison.OrdinalIgnoreCase))
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
        //private int FindOperatorIndex(string expression, string operatorStr)
        //{
        //    var regex = new Regex($@"\b{operatorStr}\b", RegexOptions.IgnoreCase);
        //    var match = regex.Match(expression);
        //    return match.Success ? match.Index : -1;
        //}

        public void PrintTree(TreeNode<string> node, int level)
        {
            Console.WriteLine(new string(' ', level * 2) + node.Value);
            foreach (var child in node.Children)
            {
                PrintTree(child, level + 1);
            }
        }

        public string CombineTree(TreeNode<string> node)
        {
            if (node.Children.Count == 0)
                return node.Value;
            else if (node.Value == "()")
            {
                return $"({CombineTree(node.Children[0])})";
            }
            else
            {
                var left = CombineTree(node.Children[0]);
                var right = CombineTree(node.Children[1]);
                return $"{left} {node.Value} {right}";
            }
        }

    }

    
}
