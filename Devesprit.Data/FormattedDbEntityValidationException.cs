using System;
using System.Data.Entity.Validation;
using System.Text;

namespace Devesprit.Data
{
    public partial class FormattedDbEntityValidationException : Exception
    {
        public FormattedDbEntityValidationException(DbEntityValidationException innerException) :
            base(null, innerException)
        {
        }

        public override string Message
        {
            get
            {
                if (InnerException is DbEntityValidationException innerException)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine();
                    sb.AppendLine();
                    foreach (var eve in innerException.EntityValidationErrors)
                    {
                        sb.AppendLine(
                            $"- Entity of type \"{eve.Entry.Entity.GetType().FullName}\" in state \"{eve.Entry.State}\" has the following validation errors:");
                        foreach (var ve in eve.ValidationErrors)
                        {
                            sb.AppendLine(
                                $"-- Property: \"{ve.PropertyName}\", Value: \"{eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName)}\", Error: \"{ve.ErrorMessage}\"");
                        }
                    }
                    sb.AppendLine();

                    return sb.ToString();
                }

                return base.Message;
            }
        }
    }
}
