namespace Beskar.Markdown.Utils;

public static class SpanUtils
{
   public static char TryParseEntity(ReadOnlySpan<char> span, out int consumed)
   {
      consumed = 0;
      if (span.Length < 3) 
         return '\0';

      var semiColonIndex = span.IndexOf(';');
      if (semiColonIndex is -1 or > 32) 
         return '\0';

      var content = span[1..semiColonIndex];

      // Handle Numeric Entities (&#10; or &#x0A;)
      if (content.Length > 1 && content[0] == '#')
      {
         var isHex = content.Length > 2 && (content[1] == 'x' || content[1] == 'X');
         var numberSpan = isHex ? content[2..] : content[1..];
      
         var style = isHex 
            ? System.Globalization.NumberStyles.HexNumber 
            : System.Globalization.NumberStyles.Integer;
      
         if (uint.TryParse(numberSpan, style, null, out var codePoint))
         {
            if (codePoint > 0x10FFFF) return '\0';

            consumed = semiColonIndex + 1;
            if (codePoint == 0) return '\uFFFD';
            return (char)codePoint; 
         }
      }
   
      var named = content switch
      {
         // Core XML / HTML
         "amp"      => '&',
         "lt"       => '<',
         "gt"       => '>',
         "quot"     => '"',
         "apos"     => '\'',
         
         // Controls and Basic Symbols
         "nbsp"     => (char)160,
         "iexcl"    => '¡',
         "cent"     => '¢',
         "pound"    => '£',
         "curren"   => '¤',
         "yen"      => '¥',
         "brvbar"   => '¦',
         "sect"     => '§',
         "uml"      => '¨',
         "copy"     => '©',
         "ordf"     => 'ª',
         "laquo"    => '«',
         "not"      => '¬',
         "shy"      => (char)173,
         "reg"      => '®',
         "macr"     => '¯',
         "deg"      => '°',
         "plusmn"   => '±',
         "sup2"     => '²',
         "sup3"     => '³',
         "acute"    => '´',
         "micro"    => 'µ',
         "para"     => '¶',
         "middot"   => '·',
         "cedil"    => '¸',
         "sup1"     => '¹',
         "ordm"     => 'º',
         "raquo"    => '»',
         "frac14"   => '¼',
         "frac12"   => '½',
         "frac34"   => '¾',
         "iquest"   => '¿',
         "euro"     => '€',
         "Dcaron"   => 'Ď',
         "HilbertSpace" => 'ℋ',
         "DifferentialD" => 'ⅆ',
         "ClockwiseContourIntegral" => '∲',

         // Latin-1 Uppercase Letters
         "Agrave"   => 'À',
         "Aacute"   => 'Á',
         "Acirc"    => 'Â',
         "Atilde"   => 'Ã',
         "Auml"     => 'Ä',
         "Aring"    => 'Å',
         "AElig"    => 'Æ',
         "Ccedil"   => 'Ç',
         "Egrave"   => 'È',
         "Eacute"   => 'É',
         "Ecirc"    => 'Ê',
         "Euml"     => 'Ë',
         "Igrave"   => 'Ì',
         "Iacute"   => 'Í',
         "Icirc"    => 'Î',
         "Iuml"     => 'Ï',
         "ETH"      => 'Ð',
         "Ntilde"   => 'Ñ',
         "Ograve"   => 'Ò',
         "Oacute"   => 'Ó',
         "Ocirc"    => 'Ô',
         "Otilde"   => 'Õ',
         "Ouml"     => 'Ö',
         "times"    => '×',
         "Oslash"   => 'Ø',
         "Ugrave"   => 'Ù',
         "Uacute"   => 'Ú',
         "Ucirc"    => 'Û',
         "Uuml"     => 'Ü',
         "Yacute"   => 'Ý',
         "THORN"    => 'Þ',
         "szlig"    => 'ß',

         // Latin-1 Lowercase Letters
         "agrave"   => 'à',
         "aacute"   => 'á',
         "acirc"    => 'â',
         "atilde"   => 'ã',
         "auml"     => 'ä',
         "aring"    => 'å',
         "aelig"    => 'æ',
         "ccedil"   => 'ç',
         "egrave"   => 'è',
         "eacute"   => 'é',
         "ecirc"    => 'ê',
         "euml"     => 'ë',
         "igrave"   => 'ì',
         "iacute"   => 'í',
         "icirc"    => 'î',
         "iuml"     => 'ï',
         "eth"      => 'ð',
         "ntilde"   => 'ñ',
         "ograve"   => 'ò',
         "oacute"   => 'ó',
         "ocirc"    => 'ô',
         "otilde"   => 'õ',
         "ouml"     => 'ö',
         "divide"   => '÷',
         "oslash"   => 'ø',
         "ugrave"   => 'ù',
         "uacute"   => 'ú',
         "ucirc"    => 'û',
         "uuml"     => 'ü',
         "yacute"   => 'ý',
         "thorn"    => 'þ',
         "yuml"     => 'ÿ',
         
         // Latin Extended-A
         "OElig"    => 'Œ',
         "oelig"    => 'œ',
         "Scaron"   => 'Š',
         "scaron"   => 'š',
         "Yuml"     => 'Ÿ',
         
         // Spacing Modifier Letters
         "circ"     => 'ˆ',
         "tilde"    => '˜',
         
         // General Punctuation
         "ensp"     => ' ',
         "emsp"     => ' ',
         "thinsp"   => ' ',
         "zwnj"     => '‌',
         "zwj"      => '‍',
         "lrm"      => '‎',
         "rlm"      => '‏',
         "ndash"    => '–',
         "mdash"    => '—',
         "lsquo"    => '‘',
         "rsquo"    => '’',
         "sbquo"    => '‚',
         "ldquo"    => '“',
         "rdquo"    => '”',
         "bdquo"    => '„',
         "dagger"   => '†',
         "Dagger"   => '‡',
         "permil"   => '‰',
         "lsaquo"   => '‹',
         "rsaquo"   => '›',
         "bull"     => '•',
         "hellip"   => '…',
         "prime"    => '′',
         "Prime"    => '″',
         "oline"    => '‾',
         "frasl"    => '⁄',

         // Greek Alphabet
         "Alpha"    => 'Α',
         "Beta"     => 'Β',
         "Gamma"    => 'Γ',
         "Delta"    => 'Δ',
         "Epsilon"  => 'Ε',
         "Zeta"     => 'Ζ',
         "Eta"      => 'Η',
         "Theta"    => 'Θ',
         "Iota"     => 'Ι',
         "Kappa"    => 'Κ',
         "Lambda"   => 'Λ',
         "Mu"       => 'Μ',
         "Nu"       => 'Ν',
         "Xi"       => 'Ξ',
         "Omicron"  => 'Ο',
         "Pi"       => 'Π',
         "Rho"      => 'Ρ',
         "Sigma"    => 'Σ',
         "Tau"      => 'Τ',
         "Upsilon"  => 'Υ',
         "Phi"      => 'Φ',
         "Chi"      => 'Χ',
         "Psi"      => 'Ψ',
         "Omega"    => 'Ω',
         "alpha"    => 'α',
         "beta"     => 'β',
         "gamma"    => 'γ',
         "delta"    => 'δ',
         "epsilon"  => 'ε',
         "zeta"     => 'ζ',
         "eta"      => 'η',
         "theta"    => 'θ',
         "iota"     => 'ι',
         "kappa"    => 'κ',
         "lambda"   => 'λ',
         "mu"       => 'μ',
         "nu"       => 'ν',
         "xi"       => 'ξ',
         "omicron"  => 'ο',
         "pi"       => 'π',
         "rho"      => 'ρ',
         "sigmaf"   => 'ς',
         "sigma"    => 'σ',
         "tau"      => 'τ',
         "upsilon"  => 'υ',
         "phi"      => 'φ',
         "chi"      => 'χ',
         "psi"      => 'ψ',
         "omega"    => 'ω',
         "thetasym" => 'ϑ',
         "upsih"    => 'ϒ',
         "piv"      => 'ϖ',

         // Mathematical and Technical Symbols
         "weierp"   => '℘',
         "image"    => 'ℑ',
         "real"     => 'ℜ',
         "trade"    => '™',
         "alefsym"  => 'ℵ',
         "larr"     => '←',
         "uarr"     => '↑',
         "rarr"     => '→',
         "darr"     => '↓',
         "harr"     => '↔',
         "crarr"    => '↵',
         "lArr"     => '⇐',
         "uArr"     => '⇑',
         "rArr"     => '⇒',
         "dArr"     => '⇓',
         "hArr"     => '⇔',
         "forall"   => '∀',
         "part"     => '∂',
         "exist"    => '∃',
         "empty"    => '∅',
         "nabla"    => '∇',
         "isin"     => '∈',
         "notin"    => '∉',
         "ni"       => '∋',
         "prod"     => '∏',
         "sum"      => '∑',
         "minus"    => '−',
         "lowast"   => '∗',
         "radic"    => '√',
         "prop"     => '∝',
         "infin"    => '∞',
         "ang"      => '∠',
         "and"      => '∧',
         "or"       => '∨',
         "cap"      => '∩',
         "cup"      => '∪',
         "int"      => '∫',
         "there4"   => '∴',
         "sim"      => '∼',
         "cong"     => '≅',
         "asymp"    => '≈',
         "ne"       => '≠',
         "equiv"    => '≡',
         "le"       => '≤',
         "ge"       => '≥',
         "sub"      => '⊂',
         "sup"      => '⊃',
         "nsub"     => '⊄',
         "sube"     => '⊆',
         "supe"     => '⊇',
         "oplus"    => '⊕',
         "otimes"   => '⊗',
         "perp"     => '⊥',
         "sdot"     => '⋅',
         "lceil"    => '⌈',
         "rceil"    => '⌉',
         "lfloor"   => '⌊',
         "rfloor"   => '⌋',
         "lang"     => '⟨',
         "rang"     => '⟩',
         "loz"      => '◊',
         "spades"   => '♠',
         "clubs"    => '♣',
         "hearts"   => '♥',
         "diams"    => '♦',

         _          => '\0'
      };

      if (named != 0)
      {
         consumed = semiColonIndex + 1;
         return named;
      }

      return '\0';
   }

   public static int CountTrailingSpaces(ReadOnlySpan<char> span)
   {
      var count = 0;
      for (var i = span.Length - 1; i >= 0; i--)
      {
         var c = span[i];
         if (c is ' ' or '\t')
         {
            count++;
         }
         else
         {
            break;
         }
      }

      return count;
   }

   public static bool IsHardBreak(ReadOnlySpan<char> span)
   {
      if (span.Length == 0) return false;
      
      var last = span[^1];
      if (last == '\t') return true;
      if (last != ' ') return false;
      
      return span.Length >= 2 && span[^2] == ' ';
   }
}