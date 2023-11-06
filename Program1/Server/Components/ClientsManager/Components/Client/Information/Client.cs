using Butterfly;

namespace server.component.clientManager.component.clientShell.information
{
    public class Client
    {
        public enum Verification
        {
            None,
            // Авторизация прошла успешно.
            Success,
        }

        private const string NAME = "Information.Client:";

        private Verification _verificationResult = Verification.None;

        private readonly IInput i_process;

        public void ReturnVerificationResult(Verification result)
        {
            if (_verificationResult.HasFlag(Verification.None))
            {
                _verificationResult = result;

                i_process.To();
            }
            else
                throw new Exception();
        }

        public bool IsSuccessVerification() => _verificationResult.HasFlag(Verification.Success);

        private int _id = -1;

        public string Login { private set; get; } = "";
        private readonly int _loginMinLength;
        private readonly int _loginMaxLength;

        private string _password = "";
        private readonly int _passwordMinLength;
        private readonly int _passwordMaxLength;

        public Client(int loginMinLength, int loginMaxLength,
            int passwordMinLength, int passwordMaxLength, IInput input_process)
        {
            _loginMinLength = loginMinLength; _loginMaxLength = loginMaxLength;
            _passwordMinLength = passwordMinLength; _passwordMaxLength = passwordMaxLength;
            i_process = input_process;
        }

        public bool SetLogin(string login, out string error)
        {
            error = NAME;

            if (Login != "")
                error += $"Вы попытались повторно назначить в поле login значение {login}.";

            if (login.Length < _loginMinLength)
                error += $"Минимально допустимая длина значение поля login {_loginMinLength}, " +
                    $"переданный параметр [{login}] имеет длину {login.Length}";

            if (login.Length > _loginMaxLength)
                error += $"Максимально допустимая длина значение поля login {_loginMaxLength}, " +
                    $"переданный параметр [{login}] имеет длину {login.Length}";

            if (error == NAME)
            {
                Login = login;

                return true;
            }
            else return false;
        }

        public bool SetPassword(string password, out string error)
        {
            error = NAME;

            if (_password != "")
                error += $"Вы попытались повторно назначить в поле password значение {password}.";

            if (password.Length < _loginMinLength)
                error += $"Минимально допустимая длина значение поля password {_passwordMinLength}, " +
                    $"переданный параметр [{password}] имеет длину {password.Length}";

            if (password.Length > _loginMaxLength)
                error += $"Максимально допустимая длина значение поля password {_passwordMaxLength}, " +
                    $"переданный параметр [{password}] имеет длину {password.Length}";

            if (error == NAME)
            {
                _password = password;

                return true;
            }
            else return false;
        }
    }
}