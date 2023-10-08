using System.Net.Sockets;
using Butterfly;

public abstract class ClientProperty : Controller.Board.LocalField<TcpClient>
{
    protected int HP, MP, RG, SH = -1;

    private int _power_physics_attack, _speed_physics_attack;
    private int _power_physics_creet_attack, _chance_physics_creet_attack;
    private int _power_magic_attack, _speed_cast_magic_attack;
    private int _power_creet_magic_attack, _chance_creet_magic_attack;
    private int _dodge_physics_attack, _resist_magic_attack, _parry_physics_attack;
    private int _speed_move, _physics_defence, _magic_defence;

    private int _gain_power_physics_attack, _gain_speed_physics_attack;
    private int _gain_power_physics_creet_attack, _gain_chance_physics_creet_attack;
    private int _gain_power_magic_attack, _gain_speed_cast_magic_attack;
    private int _gain_power_creet_magic_attack, _gain_chance_creet_magic_attack;
    private int _gain_dodge_physics_attack, _gain_resist_magic_attack, _gain_parry_physics_attack;
    private int _gain_speed_move, _gain_physics_defence, _gain_magic_defence;

    private int _debilitation_power_physics_attack, _debilitation_speed_physics_attack;
    private int _debilitation_power_physics_creet_attack, _debilitation_chance_physics_creet_attack;
    private int _debilitation_power_magic_attack, _debilitation_speed_cast_magic_attack;
    private int _debilitation_power_creet_magic_attack, _debilitation_chance_creet_magic_attack;
    private int _debilitation_dodge_physics_attack, _debilitation_resist_magic_attack, _debilitation_parry_physics_attack;
    private int _debilitation_speed_move, _debilitation_physics_defence, _debilitation_magic_defence;

    private float _positionX, _positionY, _velocityX, _velocityY = 0;

    /// <summary>
    /// Уникальный ID для капсулы.
    /// </summary>
    private const uint UNIQUE_CAPSULE_ID = 0;

    /// <summary>
    /// Максимально возможное количесво отправленых сообщений без подтверждения.
    /// </summary>
    private const uint MAX_OUTPUT_MESSAGE_COUNT = 256;

    /// <summary>
    /// Максимально возможно количесво отправленых капсул без подтверждения.
    /// </summary>
    private const uint MAX_OUTPUT_CAPSULE_COUNT = 1024;

    /// <summary>
    /// Количесво Отправленых сообщений.
    /// </summary>
    private uint OutputMessagesCount = 0;

    /// <summary>
    /// Отправленые сообщения.
    /// </summary>
    private byte[][] OutputMessages = new byte[MAX_OUTPUT_MESSAGE_COUNT][];

    /// <summary>
    /// Длина записаного сообщения в OutputMessages.
    /// </summary>
    private int[] OutputMessagesLength = new int[MAX_OUTPUT_MESSAGE_COUNT];

    private readonly Dictionary<uint, uint> IndexFromOutputMessage = new Dictionary<uint, uint>(1024);

    private void Acknoledgment(uint ack)
    {
    }

    protected void AddCapsule(byte[] capsules)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="messageLength"></param>
    protected void SetCapsule(byte[][] capsules)
    {
        // Текущий индекс.
        int index = 0;

        // Капсулы.
        // length message 1
        // id message 
        // data time 9                   
        // position 8
        // direction 1
        // DATA
        // type 1 Move
        for (int i = 0; i < capsules.Length; i++)
        {
            if (capsules[i].Length < Capsule.Header.LENGTH) break;

            // Тип капсулы.

            uint idCapsule = capsules[i][Capsule.Header.MESSAGE_ID_INDEX_1byte];
            idCapsule = (idCapsule << 24) ^ capsules[i][Capsule.Header.MESSAGE_ID_INDEX_2byte];
            idCapsule = (idCapsule << 16) ^ capsules[i][Capsule.Header.MESSAGE_ID_INDEX_3byte];
            idCapsule = (idCapsule << 8) ^ capsules[i][Capsule.Header.MESSAGE_ID_INDEX_4byte];
        }
    }
}