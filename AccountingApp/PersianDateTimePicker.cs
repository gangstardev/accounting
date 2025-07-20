using System;
using System.Globalization;
using System.Windows.Forms;

namespace AccountingApp
{
    public class PersianDateTimePicker : DateTimePicker
    {
        private PersianCalendar _persianCalendar;
        private bool _isUpdating = false;

        public PersianDateTimePicker()
        {
            _persianCalendar = new PersianCalendar();
            this.Format = DateTimePickerFormat.Custom;
            this.CustomFormat = "yyyy/MM/dd";
            this.ValueChanged += PersianDateTimePicker_ValueChanged;
        }

        private void PersianDateTimePicker_ValueChanged(object? sender, EventArgs e)
        {
            if (!_isUpdating)
            {
                _isUpdating = true;
                try
                {
                    // تبدیل تاریخ میلادی به شمسی برای نمایش
                    var persianDate = PersianDateConverter.ConvertToPersianDate(this.Value);
                    this.CustomFormat = persianDate;
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        protected override void OnValueChanged(EventArgs e)
        {
            base.OnValueChanged(e);
            
            if (!_isUpdating)
            {
                _isUpdating = true;
                try
                {
                    // نمایش تاریخ شمسی
                    var persianDate = PersianDateConverter.ConvertToPersianDate(this.Value);
                    this.CustomFormat = persianDate;
                }
                finally
                {
                    _isUpdating = false;
                }
            }
        }

        public void SetPersianDate(string persianDate)
        {
            try
            {
                var gregorianDate = PersianDateConverter.FromPersianDate(persianDate);
                this.Value = gregorianDate;
            }
            catch
            {
                this.Value = DateTime.Now;
            }
        }

        public string GetPersianDate()
        {
            return PersianDateConverter.ConvertToPersianDate(this.Value);
        }
    }
} 