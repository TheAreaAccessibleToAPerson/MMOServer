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
}