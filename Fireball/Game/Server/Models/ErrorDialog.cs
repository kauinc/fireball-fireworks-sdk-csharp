using System.Collections.Generic;

namespace Fireball.Game.Server.Models
{
    public enum DialogButtonAction
    {
        OK = 0,
        Reload = 1,
        Retry = 2,
        Redirect = 3,
        ClientScript = 4,
        Home = 5,
    }

    public class ErrorDialog
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public List<ErrorDialogButton> Buttons { get; set; }

        public ErrorDialog() { }

        public ErrorDialog(string title, string message, List<ErrorDialogButton> buttons)
        {
            Title = title;
            Message = message;
            Buttons = buttons;
        }

        public static ErrorDialog Information(string title, string message, string buttonLabel = "OK")
        {
            return new ErrorDialog(title, message, new List<ErrorDialogButton>()
            {
                 ErrorDialogButton.ButtonOk(buttonLabel)
            });
        }
    }

    public class ErrorDialogButton
    {
        public string Text { get; set; }
        public DialogButtonAction Action { get; set; }
        public string Link { get; set; } // Require for Action type Redirect/3
        public ErrorClientScript ClientScript { get; set; } //Required for Action type ClientScript/4

        public ErrorDialogButton() { }

        public static ErrorDialogButton ButtonOk(string text = "OK")
        {
            return new ErrorDialogButton()
            {
                Text = text,
                Action = DialogButtonAction.OK,
            };
        }

        public static ErrorDialogButton ButtonReload(string text = "Reload")
        {
            return new ErrorDialogButton()
            {
                Text = text,
                Action = DialogButtonAction.Reload,
            };
        }

        public static ErrorDialogButton ButtonRetry(string text = "Retry")
        {
            return new ErrorDialogButton()
            {
                Text = text,
                Action = DialogButtonAction.Retry,
            };
        }

        public static ErrorDialogButton ButtonRedirect(string text, string link)
        {
            return new ErrorDialogButton()
            {
                Text = text,
                Action = DialogButtonAction.Redirect,
                Link = link,
            };
        }

        public static ErrorDialogButton ButtonClientScript(string text, ErrorClientScript script)
        {
            return new ErrorDialogButton()
            {
                Text = text,
                Action = DialogButtonAction.ClientScript,
                ClientScript = script,
            };
        }
    }
}
