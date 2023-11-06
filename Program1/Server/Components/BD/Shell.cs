using Butterfly;

namespace server.component.BD
{
    public sealed class Shell : Controller
    {
        public struct BUS
        {
            private const string NAME = "BD:";

            public struct Impuls
            {
                /// <summary>
                /// Прослушка запущена.
                /// </summary>
                public const string START = NAME + "Start";

                /// <summary>
                /// Попытка перезапустить прослушку.
                /// </summary>
                public const string RESTART = NAME + "Restart";
            }
        }

        /// <summary>
        /// Максимальное количесво попыток перезапуска обьекта.
        /// </summary>
        private const int MAX_NUMBER_OF_ATTEMPTS_RESTARTING = 25;

        /// <summary>
        /// Номер текущей попытки перезапуска.
        /// </summary>
        private int _currentAttemptsRestarting = 0;

        private const string LOG = @"Обьект обрабатывающий запросы в BD:{0}:";

        void Construction()
        {
            listen_impuls(BUS.Impuls.START)
                .output_to((infoObj) =>
                {
#if SCL
                    _logger($"{LOG} о начале работы в свою оболочку.");
#endif

                    _currentAttemptsRestarting = 0;
                });

            listen_impuls(BUS.Impuls.RESTART)
                .output_to((infoObj) =>
                {
                    if (StateInformation.IsDestroy) return;

                    if (_currentAttemptsRestarting++ < MAX_NUMBER_OF_ATTEMPTS_RESTARTING)
                    {
#if SCL
                        _logger($"{LOG} запросил свой перезапуск " +
                            $"{_currentAttemptsRestarting}/{MAX_NUMBER_OF_ATTEMPTS_RESTARTING}");
#endif
                    }
                    else
                    {
#if SCL
                        _logger($"{LOG} запросил свой перезапуск, но доступные попытки закончились.");
#endif

                        destroy();
                    }
                });

            void Destruction()
            {
#if SCL
                _logger("Start destroy.");
#endif
            }

            void Destroyed()
            {
#if SCL
                _logger("End destroy.");
#endif
            }

#if SCL
            void _logger(string info)
                => i_componentLogger.To($"{GetExplorer()}:{info}");
#endif

        }
    }
}