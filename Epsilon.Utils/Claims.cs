using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Epsilon.Utils
{

    public class Claims
    {
        public static bool Validate(string[] user, string required)
        {
            var e = new ClaismEvaluator(user, required);
            return e.Eval();
        }
    }

    class ClaismEvaluator
    {
        private Parser _p;
        private string[] _user;
        private readonly string _required;

        public ClaismEvaluator(string[] user, string required)
        {
            _p = new Parser(required);
            _user = user;
            _required = required;
        }

        public bool Eval()
        {
            if (_p.CurrentToken == Parser.Token.EndOfInput) return true;
            return EvalRequiredExpression(Parser.Token.EndOfInput);
        }

        #region privates methods

        /// <summary>
        /// Affiches tous les tokens parsés (util pour le debug)
        /// </summary>
        /// <param name="s"></param>
        public static void DumpTokens(string s)
        {
            Console.WriteLine("Tokens in '{0}': ", s);
            Parser p = new Parser(s);
            while (p.CurrentToken != Parser.Token.EndOfInput)
            {
                Console.Write(p.CurrentToken);
                if (p.CurrentToken == Parser.Token.Number)
                {
                    Console.Write(": {0}", p.NumValue);
                    Console.WriteLine("");
                }
                p.GetNextToken();
            }
            Console.WriteLine("<eos>");
        }


        private bool EvalSubExpression()
        {
            //Skip WhiteSpace
            while (_p.Match(Parser.Token.WhiteSpace)) { }

            if (_p.Match(Parser.Token.Not))
                return !EvalSubExpression();

            bool v;
            if (_p.Match(Parser.Token.OpenPar))
                v = EvalRequiredExpression(Parser.Token.ClosePar);
            else if (_p.Match(Parser.Token.Identifier))
            {
                if (_p.Identifier[_p.Identifier.Length - 1] == '%')
                    v = _user.Any(i => i.StartsWith(_p.Identifier.Trim('%')));
                else if (_p.Identifier[0] == '%')
                    v = _user.Any(i => i.EndsWith(_p.Identifier.Trim('%')));
                else v = _user.Contains(_p.Identifier);
            }

            else throw new ApplicationException("Unexpected token");


            for (; ; )
            {
                if (_p.Match(Parser.Token.And)) v &= EvalSubExpression();
                else break;
            }
            return v;
        }


        private bool EvalRequiredExpression(Parser.Token endToken)
        {
            var v = EvalSubExpression();
            while (!_p.Match(endToken))
            {
                if (_p.Match(Parser.Token.Or)) v |= EvalSubExpression();
                else
                {
                    if (endToken == Parser.Token.ClosePar)
                        throw new ApplicationException("None opened bracket");
                    if (endToken != Parser.Token.EndOfInput)
                        throw new ApplicationException("Expect " + endToken);
                    throw new ApplicationException(_required + " : Unexpected token" + _p.CurrentToken);
                }
            }
            return v;
        }

        #endregion

    }
}
