﻿using System;
using System.Text;

namespace Winterdom.Viasfora.Util {
  public class SimpleTokenizer : ITokenizer {
    private ITextChars tc;
    private bool reachedEnd;
    private String currentToken;

    public bool AtEnd {
      get { return reachedEnd; }
    }
    public String Token {
      get { return currentToken; }
    }

    public SimpleTokenizer(String text) {
      this.tc = new StringChars(text);
    }

    public bool Next() {
      if ( tc.EndOfLine ) {
        this.currentToken = "";
        this.reachedEnd = true;
       return false;
      }
      // skip whitespace
      while ( !tc.EndOfLine && Char.IsWhiteSpace(tc.Char()) ) {
        tc.Next();
      }
      if ( tc.EndOfLine ) {
        this.currentToken = "";
        this.reachedEnd = true;
        return false;
      }
      // if it's a special character, return it on its own
      if ( !Char.IsLetterOrDigit(tc.Char()) ) {
        char ch = tc.Char();
        tc.Next();
        this.currentToken = "" + ch;
        return true;
      }
      // return the word
      StringBuilder sb = new StringBuilder();
      while ( !tc.EndOfLine ) {
        char ch = tc.Char();
        if ( !Char.IsLetterOrDigit(ch) ) break;
        tc.Next();
        sb.Append(ch);
      }
      this.currentToken = sb.ToString();
      return true;
    }
  }
}
