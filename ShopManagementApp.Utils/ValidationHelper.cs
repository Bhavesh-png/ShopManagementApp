namespace ShopManagementApp.Utils
{
    /// <summary>
    /// Helper methods for validating user input and showing messages.
    /// </summary>
    public static class ValidationHelper
    {
        // ── Validation Methods ────────────────────────────────────────────────

        /// <summary>Checks if a phone number is exactly 10 digits.</summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            string cleaned = phone.Trim().Replace(" ", "").Replace("-", "");
            return cleaned.Length == 10 && cleaned.All(char.IsDigit);
        }

        /// <summary>Checks if a string is not null or empty.</summary>
        public static bool IsNonEmpty(string? value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        /// <summary>Checks if a decimal value is greater than zero.</summary>
        public static bool IsPositiveDecimal(decimal value)
        {
            return value > 0;
        }

        /// <summary>Checks if a number is a non-negative integer.</summary>
        public static bool IsPositiveInt(int value)
        {
            return value > 0;
        }

        // ── Message Dialogs ───────────────────────────────────────────────────

        /// <summary>Shows an error/warning popup.</summary>
        public static void ShowError(string message, string title = "Validation Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>Shows a success information popup.</summary>
        public static void ShowSuccess(string message, string title = "Success")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>Shows a Yes/No confirmation dialog. Returns true if user clicks Yes.</summary>
        public static bool Confirm(string message, string title = "Confirm Action")
        {
            return MessageBox.Show(message, title,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>Shows a critical error (unexpected exceptions).</summary>
        public static void ShowException(Exception ex, string context = "")
        {
            string msg = string.IsNullOrEmpty(context)
                ? $"An error occurred:\n{ex.Message}"
                : $"Error in {context}:\n{ex.Message}";
            MessageBox.Show(msg, "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
