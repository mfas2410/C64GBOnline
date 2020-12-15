using System;
using System.Windows.Shell;

namespace C64GBOnline.Gui.Messages
{
    public sealed class ProgressBarMessage
    {
        public ProgressBarMessage(string? text = null, byte? percentComplete = null, TaskbarItemProgressState? state = null)
        {
            Text = text;
            if (percentComplete > 100) throw new ArgumentException("Cannot be larger than 100", nameof(percentComplete));
            PercentComplete = percentComplete;
            State = state;
        }

        public string? Text { get; }

        public byte? PercentComplete { get; }

        public TaskbarItemProgressState? State { get; }
    }
}