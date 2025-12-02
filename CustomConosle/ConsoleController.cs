using System;
using System.Collections.Generic;
using Ousiron.Console;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Console = Ousiron.Console.Console;

public class ConsoleController : SingletonTemplate<ConsoleController>
{
    [SerializeField] private ConsoleView _view;
    private Console _console;
    private InputController _input;
    private List<InputActionMap> _activeInputMaps = new List<InputActionMap>();

    public void Awake()
    {
        _input = new InputController();

        var invisBuff = new InvisBuff(-1);
        Guid changedMoveSpeedID = default;
        int lastCodeGhost = 0;
        List<CommandBase> commands = new List<CommandBase>()
        {
            new Command("game_sf_show", "Show the directory of the save file.", "game_sf_show",
                () =>
                {
                    _view.EnterLog(
                        $"Save file is {GameSaveLoader.GetSaveDirectory()}/{GameManager.Instance.GAMEDATA_SAVENAME}.save");
                }),
            new Command("game_sf_delete", "Delete the save file.", "game_sf_delete",
                () =>
                {
                    _view.EnterLog(
                        $"Save file was deleted. File position was {GameSaveLoader.GetSaveDirectory()}/{GameManager.Instance.GAMEDATA_SAVENAME}.save");
                }),
            new Command<int>("ghost", "Enable or disable ghost mode for player. <on/off> is 1 or 0.",
                "ghost <on/off>",
                (on) =>
                {
                    var pl = FindObjectOfType<PlayerController>();
                    if (pl == null)
                    {
                        _view.EnterLog("Player didn't find!");
                        return;
                    }

                    var buff = pl.GetComponent<BuffsProcess>();
                    var stats = pl.GetComponent<CharacterStatsProcess>();

                    switch (on)
                    {
                        case 0:
                            // if (lastCodeGhost == 0)
                                // return;
                            //set invis
                            buff.DropBuff(invisBuff);
                            //set max speed
                            stats.ReturnStat(typeof(MoveSpeedStat), changedMoveSpeedID);
                            //set collider like trigger
                            pl.InfoData.MainCollider.isTrigger = false;
                            break;
                        case 1:
                            // if (lastCodeGhost == 1)
                                // return;
                            
                            //reset last use command
                            buff.DropBuff(invisBuff);
                            //set max speed
                            stats.ReturnStat(typeof(MoveSpeedStat), changedMoveSpeedID);
                            //set collider like trigger
                            pl.InfoData.MainCollider.isTrigger = false;
                            //***
                            
                            //set invis
                            buff.ApplyBuff(invisBuff);
                            //set max speed
                            changedMoveSpeedID = stats.ChangeStat(typeof(MoveSpeedStat), 200);
                            //set collider like trigger
                            pl.InfoData.MainCollider.isTrigger = true;
                            break;
                        default:
                            _view.EnterLog(_view.UNDEFINED_ARG);
                            return;
                    }

                    lastCodeGhost = on;
                }),
            new Command("pr_getitem_knife",
                "Add a knife to the player inventory.",
                "pr_getitem_knife", () =>
                {
                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "Knife";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, 1, out var outSlotIndex);
                }),
            new Command<int>("pr_getitem_mineexplosive",
                "Add a explosive mine to the player inventory. <quantity> is quantity of item.",
                "pr_getitem_mineexplosive <quantity>", (c) =>
                {
                    if (c <= 0)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "Mine_explosive";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, c, out var outSlotIndex);
                }),
            new Command<int>("pr_getitem_minefreezing",
                "Add a freezing mine to the player inventory. <quantity> is quantity of item.",
                "pr_getitem_minefreezing <quantity>", (c) =>
                {
                    if (c <= 0)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "Mine_Freezing";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, c, out var outSlotIndex);
                }),
            new Command<int>("pr_getitem_minegas",
                "Add a gas mine to the player inventory. <quantity> is quantity of item.",
                "pr_getitem_minegas <quantity>", (c) =>
                {
                    if (c <= 0)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "Mine_gas";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, c, out var outSlotIndex);
                }),
            new Command("pr_getitem_luger",
                "Add a Luger to a the player inventory.",
                "pr_getitem_luger", () =>
                {
                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "Luger";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, 1, out var outSlotIndex);
                }),
            new Command("pr_getitem_plasmashield",
                "Add a plasma shield to a the player inventory.",
                "pr_getitem_plasmashield", () =>
                {
                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "PlasmaShield1";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, 1, out var outSlotIndex);
                }),
            new Command<int>("pr_getitem_bulletspistol",
                "Add pistol bullets to the player inventory. <quantity> is quantity of item.",
                "pr_getitem_bulletspistol <quantity>", (c) =>
                {
                    if (c <= 0)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "BulletsPistol";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, c, out var outSlotIndex);
                }),
            new Command<int>("pr_getitem_bulletremington",
                "Add remington bullets to the player inventory. <quantity> is quantity of item.",
                "pr_getitem_bulletremington <quantity>", (c) =>
                {
                    if (c <= 0)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    var itemName = "BulletsRemington";
                    var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                    inv.TryAddItem(itemData, c, out var outSlotIndex);
                }),

            new Command<int>("pr_hpadd",
                "Add hit points",
                "pr_hpadd <amount>", (c) =>
                {
                    if (c <= 0)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var hp = p.GetComponent<HitPointsProcces>();
                    if (hp == null)
                    {
                        _view.EnterLog("HpProcess is not initialized.");
                        return;
                    }

                    hp.Health += c;
                }),
            new Command<int>("lvl_loadlevel", "Load a level by ID. <ID> id of a level", "lvl_loadlevel <ID>",
                ID =>
                {
                    if (ID < 5)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    GameManager.Instance.SceneLoader.LoadScene(ID);
                }),
            new Command("lvl_levelsid", "Print available levels ID.", "lvl_levelsid",
                () =>
                {
                    _view.EnterLog("Available levels ID:");
                    _view.EnterLog("4, 5, 6, 7");
                }),
            new Command("restart", "Restart the current level.", "restart",
                () =>
                {
                    var curSceneID = SceneManager.GetActiveScene().buildIndex;
                    if (curSceneID < 4 || curSceneID > 7)
                    {
                        _view.EnterLog(_view.UNDEFINED_ARG);
                        return;
                    }

                    GameManager.Instance.SceneLoader.LoadScene(curSceneID);
                }),
            new Command("quit", "Quit from the game.", "quit",
                () => { Application.Quit(); }),
            new Command<int>("pr_getpackweapons",
                "Add a pack of 4 weapons or items to the player inventory. <ID> is ID of pack.",
                "pr_getpackweapons <ID>", (id) =>
                {
                    var p = FindObjectOfType<PlayerController>();
                    if (p == null)
                    {
                        _view.EnterLog("Player is not found.");
                        return;
                    }

                    var inv = p.GetComponent<InventoringProcces>();
                    if (inv == null)
                    {
                        _view.EnterLog("Inventory is not initialized.");
                        return;
                    }

                    switch (id)
                    {
                        case 1:
                            var lugerData = Addressables.LoadAssetAsync<ItemDataBase>("Luger").WaitForCompletion();
                            inv.TryAddItem(lugerData, 1, out _);
                            var bulletsData = Addressables.LoadAssetAsync<ItemDataBase>("BulletsPistol")
                                .WaitForCompletion();
                            inv.TryAddItem(bulletsData, 20, out _);
                            var knifeData = Addressables.LoadAssetAsync<ItemDataBase>("Knife").WaitForCompletion();
                            inv.TryAddItem(knifeData, 1, out _);
                            var explosiveMineData = Addressables.LoadAssetAsync<ItemDataBase>("Mine_explosive")
                                .WaitForCompletion();
                            inv.TryAddItem(explosiveMineData, 2, out _);
                            break;
                        case 2:
                            explosiveMineData = Addressables.LoadAssetAsync<ItemDataBase>("Mine_explosive")
                                .WaitForCompletion();
                            inv.TryAddItem(explosiveMineData, 2, out _);
                            var freezingMineData = Addressables.LoadAssetAsync<ItemDataBase>("Mine_Freezing")
                                .WaitForCompletion();
                            inv.TryAddItem(freezingMineData, 4, out _);
                            var gasMineData = Addressables.LoadAssetAsync<ItemDataBase>("Mine_gas").WaitForCompletion();
                            inv.TryAddItem(gasMineData, 2, out _);
                            break;

                        case 3:
                            var explosiveGrenadeData = Addressables.LoadAssetAsync<ItemDataBase>("Grenade_explosive")
                                .WaitForCompletion();
                            inv.TryAddItem(explosiveGrenadeData, 4, out _);
                            var freezingGrenadeData = Addressables.LoadAssetAsync<ItemDataBase>("Grenade_freezing")
                                .WaitForCompletion();
                            inv.TryAddItem(freezingGrenadeData, 2, out _);
                            var gasGrenadeData = Addressables.LoadAssetAsync<ItemDataBase>("Grenade_gas")
                                .WaitForCompletion();
                            inv.TryAddItem(gasGrenadeData, 2, out _);
                            break;
                        default:
                            _view.EnterLog(_view.UNDEFINED_ARG);
                            break;
                    }
                }),
            new Command<int>("lvl_goldadd", "Adds gold. <amount> amount of gold", "lvl_goldadd <amount>",
                amount => { GameManager.Instance.ShopTransHandler.AddGold(amount); }),
            new Command("win", "Win current level.", "win",
                () =>
                {
                    var id = SceneManager.GetActiveScene().buildIndex;
                    if (id < 4)
                    {
                        _view.EnterLog("Incorrect level");
                        return;
                    }

                    var end = FindObjectOfType<EndGameController>();
                    if (end != null)
                        end.FinishGame(EndType.Win);
                    else
                    {
                        var curLevelIndex = LevelManager.Instance.CurrentLevelIndex;
                        foreach (var l in GameManager.Instance.GameData.levels)
                        {
                            if(l.levelID <= curLevelIndex)
                                l.isPassed = true;
                        }
                        
                        // GameManager.Instance.GameData.unpassedLevelPointer++;
                        GameManager.Instance.SceneLoader.LoadScene(SceneLoader.BAR_SCENE_ID);
                    }
                }),
            new Command("pr_getitem_gentleman", "Add clothes to the player inventory", "pr_getitem_gentleman", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "ClothesGentleman0";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
            new Command("pr_getitem_soldier", "Add clothes to the player inventory", "pr_getitem_soldier", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "ClothesSoldier0";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
            new Command("pr_getitem_knight", "Add clothes to the player inventory", "pr_getitem_knight", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "ClothesKnight0";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
            new Command("pr_getitem_hammer", "Add a hammer to the player inventory", "pr_getitem_hammer", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "Hammer_GV703";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
            new Command("pr_getitem_remington", "Add a remington to the player inventory", "pr_getitem_remington", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "Remington";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
            new Command("pr_getitem_raygun", "Add a raygun to the player inventory", "pr_getitem_raygun", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "RayGun";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
            new Command("pr_getitem_gewehr", "Add a gewehr to the player inventory", "pr_getitem_gewehr", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "Gewehr";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
            new Command("pr_getitem_shield", "Add a bracer to the player inventory", "pr_getitem_shield", () =>
            {
                var p = FindObjectOfType<PlayerController>();
                if (p == null)
                {
                    _view.EnterLog("Player is not found.");
                    return;
                }

                var inv = p.GetComponent<InventoringProcces>();
                if (inv == null)
                {
                    _view.EnterLog("Inventory is not initialized.");
                    return;
                }

                var itemName = "PlasmaShield1";
                var itemData = Addressables.LoadAssetAsync<ItemDataBase>(itemName).WaitForCompletion();
                inv.TryAddItem(itemData, 1, out var outSlotIndex);
            }),
        };

        _console = new Console(commands, _view);

        _input.Console.Enable();
        _input.Console.OpenConsole.started += ctx =>
        {
            _view.SwitchConsole(!_view.IsEnabled);
            if (_view.IsEnabled)
                SwitchConsoleWindow(true);
            else
                SwitchConsoleWindow(false);
        };
        _input.Console.Enter.started += ctx => { _console.OnEnterPressed(); };
        _input.Console.Tab.started += ctx => { _console.OnTabPressed(); };
        _input.Console.ArrowUp.started += ctx => { _console.OnArrowUpPressed(); };
        _input.Console.ArrowDown.started += ctx => { _console.OnArrowDownPressed(); };
    }

    public void Log(string message)
    {
        _view.EnterLog(message);
    }

    private void SwitchConsoleWindow(bool enable)
    {
        var inp = FindObjectOfType<MultiDeviceInput>()?.InputCtrl;
        if (inp == null)
            return;
        
        if (enable)
        {
            foreach (var map in inp.asset.actionMaps)
            {
                if (!map.enabled)
                    continue;

                map.Disable();
                _activeInputMaps.Add(map);
            }
        }
        else
        {
            foreach (var map in _activeInputMaps)
            {
                map.Enable();
            }

            _activeInputMaps.Clear();
        }
    }
}