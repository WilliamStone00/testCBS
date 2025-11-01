using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.Pendingaccounts
{
    /// <summary>
    /// Calculates which positions of a 12-digit account code a child may manage,
    /// based on the parent account:
    /// - Positions are 1..12; management starts at position 7 (index 6).
    /// - If the parent has any NON-zero in positions 7..12,
    ///   the child may only manage the remaining zeros AFTER the LAST non-zero.
    /// - If the parent has all zeros from positions 7..12,
    ///   the child may manage all those 6 positions.
    /// </summary>
    public static class AccountManagementPositionCalculator
    {
        public const int TotalLength = 12; // always work on the first 12 digits
        public const int StartIndex = 6;  // 0-based; position 7

        /// <summary>
        /// Calculate the management window and fixed prefix from a parent account.
        /// Throws if the parent contains fewer than 12 digits.
        /// </summary>
        public static AccountManagementConfig Calculate(string parentAccount)
        {
            var digits = ExtractDigits(parentAccount);
            if (digits.Length < TotalLength)
                throw new ArgumentException("Parent account must contain at least 12 digits.", nameof(parentAccount));

            var acc12 = digits.Substring(0, TotalLength);
            var firstSix = acc12.Substring(0, StartIndex);       // positions 1..6
            var tail6 = acc12.Substring(StartIndex);          // positions 7..12

            // Find last non-zero within positions 7..12 (tail)
            var lastNonZeroTailIdx = -1; // -1 => none
            for (int i = 0; i < tail6.Length; i++)
            {
                if (tail6[i] != '0') lastNonZeroTailIdx = i;
            }

            string prefix;
            int editableLen;
            int editableStartPos;

            if (lastNonZeroTailIdx == -1)
            {
                // All zeros in positions 7..12 -> child can manage all 6
                prefix = firstSix;
                editableLen = tail6.Length;     // 6
                editableStartPos = 7;           // positions 7..12
            }
            else
            {
                // Lock everything up to and including the last non-zero
                var fixedTail = tail6.Substring(0, lastNonZeroTailIdx + 1);
                prefix = firstSix + fixedTail;

                editableLen = tail6.Length - (lastNonZeroTailIdx + 1);
                // Convert to 1-based position: (6 fixed) + (lastNonZeroTailIdx + 1 fixed) + 1 start
                editableStartPos = StartIndex + lastNonZeroTailIdx + 2;
            }

            return new AccountManagementConfig(
                ParentFirst12: acc12,
                Prefix: prefix,
                EditableLength: editableLen,
                EditableStartPosition: editableStartPos
            );
        }

        /// <summary>
        /// Compose a valid 12-digit child code from the parent and the child-entered tail.
        /// Excess digits in the tail are truncated; missing digits are right-padded with '0'.
        /// </summary>
        public static string ComposeChildCode(string parentAccount, string childTailDigits)
        {
            var cfg = Calculate(parentAccount);

            var tail = ExtractDigits(childTailDigits);
            if (tail.Length > cfg.EditableLength)
                tail = tail.Substring(0, cfg.EditableLength);

            var composed = (cfg.Prefix + tail).PadRight(TotalLength, '0')
                                             .Substring(0, TotalLength);
            return composed;
        }

        /// <summary>
        /// Keep digits only.
        /// </summary>
        private static string ExtractDigits(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var sb = new StringBuilder(value.Length);
            foreach (var ch in value)
                if (char.IsDigit(ch)) sb.Append(ch);
            return sb.ToString();
        }
    }

    /// <summary>
    /// Result describing what the child may edit.
    /// </summary>
    public class AccountManagementConfig
    {
        public string ParentFirst12 { get; set; }
        public string Prefix { get; set; }
        public int EditableLength { get; set; }
        public int EditableStartPosition { get; set; }
       
        public AccountManagementConfig(string ParentFirst12, string Prefix, int EditableLength, int EditableStartPosition) { 
          this.ParentFirst12 = ParentFirst12;
            this.Prefix = Prefix;
            this.EditableLength = EditableLength;
            this.EditableStartPosition = EditableStartPosition;

        }
        
        
   }
        


}
