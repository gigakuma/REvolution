using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace REvolution.Core
{
    public static class CharUtils
    {
        private const byte Q = 5;    // quantifier
        private const byte S = 4;    // ordinary stoppper
        private const byte Z = 3;    // ScanBlank stopper
        private const byte X = 2;    // whitespace
        private const byte E = 1;    // should be escaped

        /*
         * For categorizing ascii characters.
        */
        private static readonly byte[] _category = new byte[] {
            // 0 1 2 3 4 5 6 7 8 9 A B C D E F 0 1 2 3 4 5 6 7 8 9 A B C D E F 
               0,0,0,0,0,0,0,0,0,X,X,0,X,X,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            //   ! " # $ % & ' ( ) * + , - . / 0 1 2 3 4 5 6 7 8 9 : ; < = > ? 
               X,0,0,Z,S,0,0,0,S,S,Q,Q,0,0,S,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,Q,
            // @ A B C D E F G H I J K L M N O P Q R S T U V W X Y Z [ \ ] ^ _
               0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,S,S,0,S,0,
            // ' a b c d e f g h i j k l m n o p q r s t u v w x y z { | } ~ 
               0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,Q,S,0,0,0};

        /*
         * Returns true for those characters that terminate a string of ordinary chars.
         */
        public static bool IsSpecial(char ch)
        {
            return (ch <= '|' && _category[ch] >= S);
        }

        /*
         * Returns true for those characters that terminate a string of ordinary chars.
         */
        public static bool IsStopperX(char ch)
        {
            return (ch <= '|' && _category[ch] >= X);
        }

        /*
         * Returns true for those characters that begin a quantifier.
         */
        public static bool IsQuantifier(char ch)
        {
            return (ch <= '{' && _category[ch] >= Q);
        }

        /*
         * Returns true for whitespace.
         */
        public static bool IsSpace(char ch)
        {
            return (ch <= ' ' && _category[ch] == X);
        }

        /*
         * Returns true for chars that should be escaped.
         */
        public static bool IsMetachar(char ch)
        {
            return (ch <= '|' && _category[ch] >= E);
        }

        public static string EscapeWhiteSpace(char ch)
        {
            if (!IsSpace(ch))
                return null;
            char esc;
            switch (ch)
            {
                case '\n':
                    esc = 'n';
                    break;
                case '\r':
                    esc = 'r';
                    break;
                case '\t':
                    esc = 't';
                    break;
                case '\f':
                    esc = 'f';
                    break;
                case ' ':
                    return "\\x20";
                default:
                    esc = '\0';
                    break;
            }
            return "\\" + esc;
        }

        public static bool IsSymbolsChar(char ch)
        {
            //return IsWordChar(ch) || ch == '\x20';
            return ch != ':';
        }

        public static bool IsWordChar(char ch)
        {
            return ((uint)(ch - '0') <= 9 || (uint)(ch - 'a') <= 25 || (uint)(ch - 'A') <= 25 || ch == '_');
        }
    }
}
