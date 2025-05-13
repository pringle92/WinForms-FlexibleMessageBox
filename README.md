# FlexibleMessageBox for .NET WinForms

A flexible and feature-rich replacement for the standard .NET `System.Windows.Forms.MessageBox`. This control offers enhanced customization, including resizable dialogs, RTF support, input fields, progress bars, custom styling, and a fluent builder pattern for easy configuration.

**Original Author:** Jörg Reichert (public@jreichert.de)  
**Contributors:** David Hall, Roink  
**RTF Mod Author:** Chris Pringle  
**Enhanced Version (v2.0) Author:** Chris Pringle

## Key Features (Version 2.0)

* **Fluent Builder Pattern:** Easily configure message boxes using `FlexibleMessageBox.Create().SetText(...).Show();`.
* **Resizable Dialogs:** Users can resize the message box, and content (text or RTF) word-wraps accordingly.
* **Rich Text Format (RTF) Support:** Display messages with advanced formatting like bold, italics, colors, etc.
* **Asynchronous Operations:** `ShowAsync()` methods for non-blocking UI in async/await applications.
* **Customizable Styling:** Control font, colors (background, text, buttons), and more using a `MessageBoxStyle` object.
* **Input Fields:** Display a `TextBox` to capture user input.
* **"Don't Show This Again" Checkbox:** Optional checkbox for persistent user preferences.
* **Progress Bar:** Display a `ProgressBar` for short operations.
* **Timeout/Auto-Close:** Configure message boxes to close automatically after a set duration.
* **Customizable Buttons:**
    * Define custom button texts.
    * Assign callback actions to buttons.
* **Custom Icons:** Use standard system icons or provide your own `Image` object.
* **System Sounds:** Plays standard system sounds associated with message box icons.
* **Hyperlink Click Event:** Custom handling for hyperlink clicks within the message.
* **Expanded Localization:** Easily add or modify button text translations.
* **Comprehensive Return Value:** `FlexibleDialogResult` provides dialog result, input text, and checkbox state.
* **Improved Auto-Sizing:** Dynamically adjusts size to fit content and UI elements.
* **Accessibility Improvements:** Enhanced accessibility for various components.
* **Event Handling Correction:** Resolved issues with `HyperlinkClickedEvent` invocation, ensuring custom handlers are correctly called from the builder.
* **Accessibility Fixes (CS0052, CS0051):** Changed accessibility of `FlexibleMessageBoxForm`, `LanguageID`, and `ButtonID` to `public` to resolve inconsistent accessibility errors when using the builder with custom callbacks or accessing `ButtonTexts`.
* **`ProgressBarStyle` Default Fix (CS0070):** Corrected the default `ProgressBarStyle` in the builder and `AddProgressBar` method to use `System.Windows.Forms.ProgressBarStyle.Blocks` instead of a non-existent value.
* **`CheckBox.PreferredWidth` Fix (CS1061):** Corrected layout logic to use `CheckBox.Width` (as `AutoSize` is true) instead of a non-existent `PreferredWidth` property.

## How to Use

### 1. Add to Your Project

Simply add the `FlexibleMessageBox.cs` file to your .NET WinForms project.

### 2. Basic Usage (Similar to Standard MessageBox)

For quick messages, you can use the static `Show` methods:

```csharp
// Simplest message
FlexibleMessageBox.Show("This is a simple message.");

// With caption
FlexibleMessageBox.Show("This is the message text.", "My Application Title");

// With caption and buttons
FlexibleMessageBox.Show("Do you want to save changes?", "Save Confirmation", MessageBoxButtons.YesNo);

// With caption, buttons, and icon
FlexibleMessageBox.Show("Operation completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

// All basic parameters
FlexibleDialogResult result = FlexibleMessageBox.Show(
    owner: this, // Optional: to center on parent form
    text: "Are you sure you want to delete this item?",
    caption: "Confirm Deletion",
    buttons: MessageBoxButtons.YesNo,
    icon: MessageBoxIcon.Warning,
    defaultButton: MessageBoxDefaultButton.Button2 // 'No' button is default
);

if (result.DialogResult == DialogResult.Yes)
{
    // Perform deletion
}
3. Using the Fluent Builder (Recommended for Advanced Features)The builder pattern provides a clean and readable way to configure all features.FlexibleDialogResult result = FlexibleMessageBox.Create()
    .SetCaption("User Registration")
    .SetText("Please enter your desired username and password.")
    .WithIcon(MessageBoxIcon.Information)
    .AddTextBox("Username:", defaultValue: "User123") // Adds an input field
    .AddTextBox("Password:", isPassword: true)        // Adds a password field
    .WithButtons(MessageBoxButtons.OKCancel)
    .Show(this); // 'this' is the owner form

if (result.DialogResult == DialogResult.OK)
{
    string username = result.InputText; // InputText from the first textbox
    // Note: To get multiple inputs, you'd need to extend FlexibleDialogResult or use callbacks.
    // For now, result.InputText will contain the text from the *last* added textbox if multiple are used without custom handling.
    // A better way for multiple inputs is to use callbacks to retrieve values.
}
4. Feature Examples with BuilderRTF Textstring rtf = @"{\rtf1\ansi This is \b bold\b0 and this is \i italic\i0.}";
FlexibleMessageBox.Create()
    .SetCaption("RTF Example")
    .SetRtfText(rtf)
    .Show();
Custom Icons// Using a standard SystemIcon
FlexibleMessageBox.Create()
    .SetText("Information with standard icon.")
    .WithIcon(MessageBoxIcon.Information)
    .Show();

// Using your own Image object
Image myCustomIcon = Image.FromFile("path/to/my_icon.png");
FlexibleMessageBox.Create()
    .SetText("Message with a custom icon.")
    .WithCustomIcon(myCustomIcon)
    .Show();
Input FieldsFlexibleDialogResult inputResult = FlexibleMessageBox.Create()
    .SetCaption("Feedback")
    .SetText("Please provide your feedback:")
    .AddTextBox("Your comments:") // Label for the textbox
    .WithButtons(MessageBoxButtons.OKCancel)
    .Show();

if (inputResult.DialogResult == DialogResult.OK && !string.IsNullOrEmpty(inputResult.InputText))
{
    Console.WriteLine($"Feedback received: {inputResult.InputText}");
}
To get multiple input values, it's best to use button callbacks to access the TextBox controls directly on the form instance passed to the callback. FlexibleDialogResult.InputText will only contain the value of the last added TextBox."Don't Show This Again" CheckboxFlexibleDialogResult checkResult = FlexibleMessageBox.Create()
    .SetText("This is an important tip you might want to see again.")
    .AddDontShowAgainCheckBox("Do not show this tip anymore", defaultChecked: false)
    .Show();

if (checkResult.DontShowAgainChecked)
{
    // Save user preference
}
Progress BarFlexibleMessageBox.Create()
    .SetCaption("Working...")
    .SetText("Processing your request, please wait.")
    .AddProgressBar(min: 0, max: 100, value: 50, style: ProgressBarStyle.Continuous)
    .WithButtons(MessageBoxButtons.OK) // Or no buttons if it auto-closes
    .Show();
Note: The progress bar is static in this example. For a dynamic progress bar, you would need to manage the FlexibleMessageBoxForm instance and update the ProgressBar control directly.Timeout / Auto-CloseFlexibleMessageBox.Create()
    .SetText("This message will self-destruct in 5 seconds...")
    .SetTimeout(5000, DialogResult.Abort) // Closes after 5s, returns Abort
    .Show();
Custom Button Text & CallbacksFlexibleMessageBox.Create()
    .SetCaption("Confirmation")
    .SetText("Choose an action:")
    .WithButtons(MessageBoxButtons.YesNoCancel) // Defines which buttons are present
    .SetButtonText(FlexibleMessageBox.ButtonID.YES, "Accept")
    .SetButtonText(FlexibleMessageBox.ButtonID.NO, "Decline")
    .SetButtonText(FlexibleMessageBox.ButtonID.CANCEL, "Later")
    .SetButtonCallback(FlexibleMessageBox.ButtonID.YES, (formInstance, dialogResult) => 
    {
        Console.WriteLine("Accepted!");
        // Access formInstance.Controls if needed, e.g., formInstance.textBoxInput.Text
    })
    .SetButtonCallback(FlexibleMessageBox.ButtonID.NO, (formInstance, dialogResult) => 
    {
        Console.WriteLine("Declined!");
    })
    .Show();
Stylingvar customStyle = new MessageBoxStyle
{
    Font = new Font("Segoe UI", 10f),
    BackColor = Color.LightSteelBlue,
    TextColor = Color.DarkSlateGray,
    ButtonBackColor = Color.SlateGray,
    ButtonForeColor = Color.White,
    MessageBackColor = Color.Azure
};

FlexibleMessageBox.Create()
    .SetText("This is a styled message box!")
    .WithStyle(customStyle)
    .Show();
Asynchronous Operationspublic async Task ShowMyMessageAsync()
{
    FlexibleDialogResult asyncResult = await FlexibleMessageBox.Create()
        .SetCaption("Async Operation")
        .SetText("This dialog was shown asynchronously.")
        .ShowAsync();
    // Continue after dialog is closed
    Console.WriteLine($"Async dialog closed with: {asyncResult.DialogResult}");
}
Hyperlink HandlingFlexibleMessageBox.Create()
    .SetText("Please visit our website: [www.example.com](https://www.example.com) for more info.")
    .AddHyperlinkClickHandler((sender, e) =>
    {
        Console.WriteLine($"Link clicked: {e.LinkText}");
        // Custom handling, e.g., open in an internal browser or log
        // To prevent default browser opening, you might need to modify the
        // RichTextBoxMessage_LinkClicked method in FlexibleMessageBoxForm
        // to check if the event was "handled" by a custom subscriber.
        // For now, custom handlers are invoked, then default behavior follows.
    })
    .Show();
5. LocalizationButton texts can be localized. The FlexibleMessageBox.ButtonTexts dictionary is public static readonly but its content (the string arrays) can be modified if needed, or new LanguageID entries can be added.// Example: Adding French translations (before showing any message box)
// Ensure FlexibleMessageBox.LanguageID enum has 'fr'
// FlexibleMessageBox.ButtonTexts.Add(FlexibleMessageBox.LanguageID.fr, new[] { "OK", "Annuler", "&Oui", "&Non", ... }); 
The message box automatically uses CultureInfo.CurrentUICulture to select the language.CreditsOriginal Author: Jörg ReichertContributors: David Hall, RoinkRTF Mod Author: Chris PringleEnhanced Version (v2.0): Chris
