using System;

namespace SDKManagement
{
    public interface ISDKReporter
    {
        /// <summary>
        /// Колбэк по завершению генерации SDKReport
        /// </summary>
        Action<SDKReport> OnReportComplete
        {
            get;
            set;
        }
        /// <summary>
        /// Сгенерировать SDKReport
        /// </summary>
        void GenerateReport();
        /// <summary>
        /// Есть ли кастомная кнопка   
        /// </summary>
        bool hasButton
        {
            get;
        }
        /// <summary>
        /// Действие по нажатию на кастомную кнопку
        /// </summary>
        void ButtonClick();
        /// <summary>
        /// Текст на кастомной кнопке
        /// </summary>
        string buttonLabel
        {
            get;
        }
    }
}