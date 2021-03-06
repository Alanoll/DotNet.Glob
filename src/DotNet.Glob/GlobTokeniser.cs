﻿using System;
using System.Collections.Generic;
using System.Text;
using DotNet.Globbing.Token;

namespace DotNet.Globbing
{
    public class GlobTokeniser
    {

        private StringBuilder _currentBufferText;

        public GlobTokeniser()
        {
            _currentBufferText = new StringBuilder();
        }

        public IList<IGlobToken> Tokenise(string globText, bool allowInvalidPathCharcaters)
        {
            var tokens = new List<IGlobToken>();
            using (var reader = new GlobStringReader(globText))
            {
                while (reader.ReadChar())
                {
                    if (reader.IsBeginningOfRangeOrList)
                    {
                        tokens.Add(ReadRangeOrListToken(reader));
                    }
                    else if (reader.IsSingleCharacterMatch)
                    {
                        tokens.Add(ReadSingleCharacterMatchToken());
                    }
                    else if (reader.IsWildcardCharacterMatch)
                    {
                        tokens.Add(ReadWildcardToken());
                    }
                    else if (reader.IsPathSeperator())
                    {
                        var sepToken = ReadPathSeperatorToken(reader);
                        tokens.Add(sepToken);

                    }
                    else if (reader.IsBeginningOfDirectoryWildcard)
                    {
                        if (tokens.Count > 0)
                        {
                            var lastToken = tokens[tokens.Count - 1] as PathSeperatorToken;

                            if (lastToken != null)
                            {
                                tokens.Remove(lastToken);
                                tokens.Add(ReadDirectoryWildcardToken(reader, lastToken));
                                continue;
                            }
                        }

                        tokens.Add(ReadDirectoryWildcardToken(reader, null));
                    }
                    else
                    {
                        tokens.Add(ReadLiteralToken(reader, allowInvalidPathCharcaters));

                        ////else if (reader.IsValidLiteralCharacter())
                        ////{
                        //// literal
                        //tokens.Add(ReadLiteralToken(reader));
                    }
                }
            }

            _currentBufferText.Clear();

            return tokens;

        }

        private IGlobToken ReadDirectoryWildcardToken(GlobStringReader reader, PathSeperatorToken leadingPathSeperatorToken)
        {
            reader.ReadChar();

            if (GlobStringReader.IsPathSeperator(reader.PeekChar()))
            {
                reader.ReadChar();
                var trailingSeperator = ReadPathSeperatorToken(reader);
                return new WildcardDirectoryToken(leadingPathSeperatorToken, trailingSeperator);
            }

            return new WildcardDirectoryToken(leadingPathSeperatorToken, null); // this shouldn't happen unless a pattern ends with ** which is weird. **sometext is not legal.

        }

        private IGlobToken ReadLiteralToken(GlobStringReader reader, bool allowAnyChracter)
        {
            bool isValid = allowAnyChracter;
            if (!allowAnyChracter)
            {
                isValid = GlobStringReader.IsValidLiteralCharacter(reader.CurrentChar);
                if (!isValid)
                {
                    throw new NotSupportedException($"{reader.CurrentChar} is not a supported character for a pattern.");
                }
            }
            //else
            //{
            //    // dont need to check this because if this was start of token, token would havebeen parsed already, as parsing literal always called last.

            //  //  isValid = GlobStringReader.IsNotStartOfToken(reader.CurrentChar);

            //}

            //if (isValid)
            //{
                AcceptCurrentChar(reader);

                while (!reader.HasReachedEnd)
                {
                    var peekChar = reader.PeekChar();
                    if (!allowAnyChracter)
                    {
                        isValid = GlobStringReader.IsValidLiteralCharacter(peekChar);
                    }
                    else
                    {
                        isValid = GlobStringReader.IsNotStartOfToken(peekChar);
                    }

                    if (isValid)
                    {
                        if (reader.ReadChar())
                        {
                            AcceptCurrentChar(reader);
                        }
                        else
                        {
                            // potentially hit end of string.
                            break;
                        }
                    }
                    else
                    {
                        // we have hit a character that may not be a valid literal (could be unsupported, or start of a token for instance).
                        break;
                    }
                }
            

            return new LiteralToken(GetBufferAndReset());
        }

        /// <summary>
        /// Parses a token for a range or list globbing expression.
        /// </summary>
        private IGlobToken ReadRangeOrListToken(GlobStringReader reader)
        {
            bool isNegated = false;
            bool isNumberRange = false;
            bool isLetterRange = false;
            bool isCharList = false;

            if (reader.PeekChar() == GlobStringReader.ExclamationMarkChar)
            {
                isNegated = true;
                reader.Read();
            }

            var nextChar = reader.PeekChar();
            if (Char.IsLetterOrDigit(nextChar))
            {
                reader.Read();
                nextChar = reader.PeekChar();
                if (nextChar == GlobStringReader.DashChar)
                {
                    if (Char.IsLetter(reader.CurrentChar))
                    {
                        isLetterRange = true;
                    }
                    else
                    {
                        isNumberRange = true;
                    }
                    //  throw new ArgumentOutOfRangeException("Range expressions must either be a letter range, i.e [a-z] or a number range i.e [0-9]");
                }
                else
                {
                    isCharList = true;
                }

                AcceptCurrentChar(reader);
            }
            else
            {
                isCharList = true;
                reader.Read();
                AcceptCurrentChar(reader);
            }

            if (isLetterRange || isNumberRange)
            {
                // skip over the dash char
                reader.ReadChar();
            }

            while (reader.ReadChar())
            {
                //  ReadCharacter(CharacterType.BracketedText, CurrentChar);
                if (reader.IsEndOfRangeOrList)
                {
                    var peekChar = reader.PeekChar();
                    // Close brackets within brackets are escaped with another
                    // Close bracket. e.g. [a]] matches a[
                    if (peekChar == GlobStringReader.CloseBracketChar)
                    {
                        AcceptCurrentChar(reader);
                        // Read();
                        //ReadCharacter(CharacterType.BracketedText, CurrentChar);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    AcceptCurrentChar(reader);
                }
            }

            // construct token
            IGlobToken result = null;
            var value = GetBufferAndReset();
            if (isCharList)
            {
                result = new CharacterListToken(value.ToCharArray(), isNegated);
            }
            else if (isLetterRange)
            {
                var start = value[0];
                var end = value[1];
                result = new LetterRangeToken(start, end, isNegated);
            }
            else if (isNumberRange)
            {
                var start = value[0]; // int.Parse(value[0].ToString());
                var end = value[1]; // int.Parse(value[1].ToString());
                result = new NumberRangeToken(start, end, isNegated);
            }

            return result;


        }

        private PathSeperatorToken ReadPathSeperatorToken(GlobStringReader reader)
        {
            return new PathSeperatorToken(reader.CurrentChar);
        }

        private IGlobToken ReadWildcardToken()
        {
            return new WildcardToken();
        }

        private IGlobToken ReadSingleCharacterMatchToken()
        {
            // this.Read();
            return new AnyCharacterToken();
        }

        private void AcceptChar(char character)
        {
            _currentBufferText.Append(character);
        }

        private void AcceptCurrentChar(GlobStringReader reader)
        {
            _currentBufferText.Append(reader.CurrentChar);
        }

        private string GetBufferAndReset()
        {
            var text = _currentBufferText.ToString();
            _currentBufferText.Clear();
            return text;
        }

    }
}
