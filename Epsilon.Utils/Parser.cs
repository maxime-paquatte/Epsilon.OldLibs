using System;
using System.Text;

namespace Epsilon.Utils
{
    public class Parser
    {
        #region internal classes

        /// <summary>
        /// Liste des parties de l'expression à évaluer
        /// </summary>
        [Flags]
        public enum Token : short
        {
            None = 0,
            IsAddOperator = 1,
            Plus = IsAddOperator,
            Minus = IsAddOperator + 2,
            IsMultOperator = 4,
            Mult = IsMultOperator,
            Div = IsMultOperator + 2,
            Number = 8,
            OpenPar = 16,
            ClosePar = 32,
            Or = 64,
            And = 128,
            Not = 256,

            WhiteSpace = 2048,
            Identifier = 4096,
            Error = 8192,
            EndOfInput = 16384,
        }

        #endregion

        #region private fields

        int _pos;
        Token _curToken;
        string _toParse;
        double _numValue;
        string _identifier;
        StringBuilder _buffer;

        #endregion

        #region properties

        /// <summary>
        /// Le token précédament lu
        /// </summary>
        public Token CurrentToken
        {
            get { return _curToken; }
        }

        public double NumValue
        {
            get { return _numValue; }
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        bool IsEnd
        {
            get { return _pos >= _toParse.Length; }
        }


        #endregion

        #region constructors

        public Parser(String s)
        {
            _buffer = new StringBuilder();
            _toParse = s;
            _pos = 0;
            _curToken = Token.None;
            GetNextToken();
        }

        #endregion

        #region public methods

        public Token GetNextToken()
        {
            if (IsEnd) return _curToken = Token.EndOfInput;

            char c = Read();
            while (Char.IsWhiteSpace(c))
            {
                if (IsEnd) return _curToken = Token.EndOfInput;
                c = Read();
            }
            switch (c)
            {
                case '+': _curToken = Token.Plus; break;
                case '-': _curToken = Token.Minus; break;
                case '*': _curToken = Token.Mult; break;
                case '/': _curToken = Token.Div; break;
                case '(': _curToken = Token.OpenPar; break;
                case ')': _curToken = Token.ClosePar; break;
                case '|': _curToken = Token.Or; break;
                case '&': _curToken = Token.And; break;
                case '!': _curToken = Token.Not; break;
                case ' ': _curToken = Token.WhiteSpace; break;
                default:
                    {
                        if (Char.IsDigit(c))
                        {
                            _curToken = Token.Number;
                            double val = (int)(c - '0');
                            while (!IsEnd && Char.IsDigit(c = Peek()))
                            {
                                val = val * 10 + (int)(c - '0');
                                Forward();
                            }
                            _numValue = val;
                        }
                        else if (Char.IsLetter(c) || c == '_' || c == '.' || c == '%')
                        {
                            _curToken = Token.Identifier;
                            _buffer.Length = 0;
                            _buffer.Append(c);
                            while (!IsEnd
                                    && (Char.IsLetterOrDigit(c = Peek()) || c == '_' || c == '.' || c == '%'))
                            {
                                _buffer.Append(c);
                                Forward();
                            }
                            _identifier = _buffer.ToString();
                        }
                        else _curToken = Token.Error;
                        break;
                    }
            }
            return _curToken;

        }

        public bool Match(Token t)
        {
            if (_curToken == t)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        public bool IsNumber(out double numValue)
        {
            numValue = _numValue;
            if (_curToken == Token.Number)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        public bool IsIdentifier(out string identifier)
        {
            identifier = _identifier;
            if (_curToken == Token.Identifier)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        public bool MatchIdentifier(string identifier)
        {
            if (_curToken == Token.Identifier && identifier == _identifier)
            {
                GetNextToken();
                return true;
            }
            return false;
        }

        #endregion

        #region private methods

        char Peek()
        {
            return _toParse[_pos];
        }

        char Read()
        {
            return _toParse[_pos++];
        }

        void Forward()
        {
            ++_pos;
        }

        #endregion

        #region statics methods

        static public double EvalSum(Parser p)
        {
            double r = 0;
            double term;
            while (p.IsNumber(out term))
            {
                r += term;
                if (!p.Match(Parser.Token.Plus)) break;
            }
            return r;
        }

        #endregion

    }
}
