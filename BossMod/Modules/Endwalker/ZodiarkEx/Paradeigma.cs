﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace BossMod.Endwalker.ZodiarkEx
{
    using static BossModule;

    // state related to paradeigma and astral flow mechanics
    class Paradeigma : Component
    {
        public enum FlowDirection { None, CW, CCW };

        private ZodiarkEx _module;
        private FlowDirection _flow;
        private List<Vector3> _birds = new();
        private List<Vector3> _behemoths = new();
        private List<Vector4> _snakes = new();
        private List<Vector3> _fireLine = new();

        private static float _birdBehemothOffset = 10.5f;
        private static float _snakeNearOffset = 5.5f;
        private static float _snakeFarOffset = 15.5f;
        private static float _snakeOrthoOffset = 21;
        private static AOEShapeDonut _birdAOE = new(5, 15);
        private static AOEShapeCircle _behemothAOE = new(15);
        private static AOEShapeRect _snakeAOE = new(42, 5.5f);

        public Paradeigma(ZodiarkEx module)
        {
            _module = module;
        }

        public override void AddHints(int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (_birds.Select(RotatedPosition).Any(b => _birdAOE.Check(actor.Position, b, 0)) || _behemoths.Select(RotatedPosition).Any(b => _behemothAOE.Check(actor.Position, b, 0)))
                hints.Add("GTFO from bird/behemoth aoe!");
            if (_snakes.Select(RotatedPosRot).Any(s => _snakeAOE.Check(actor.Position, new(s.X, s.Y, s.Z), s.W)))
                hints.Add("GTFO from snake aoe!");
            if (_fireLine.Any(c => GeometryUtils.PointInCone(actor.Position - _module.Arena.WorldCenter, Vector3.Normalize((c + RotatedPosition(c)) / 2 - _module.Arena.WorldCenter), MathF.PI / 4)))
                hints.Add("GTFO from fire aoe!");
        }

        public override void DrawArenaBackground(int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var b in _birds.Select(RotatedPosition))
                _birdAOE.Draw(arena, b, 0);
            foreach (var b in _behemoths.Select(RotatedPosition))
                _behemothAOE.Draw(arena, b, 0);
            foreach (var s in _snakes.Select(RotatedPosRot))
                _snakeAOE.Draw(arena, new(s.X, s.Y, s.Z), s.W);
            foreach (var c in _fireLine)
                arena.ZoneTri(c, RotatedPosition(c), arena.WorldCenter, arena.ColorAOE);
        }

        public override void DrawArenaForeground(int pcSlot, Actor pc, MiniArena arena)
        {
            if (_fireLine.Count == 2)
                arena.AddLine(_fireLine[0], _fireLine[1], arena.ColorDanger);
        }

        public override void OnEventEnvControl(uint featureID, byte index, uint state)
        {
            if (featureID != 0x80034E71)
                return;

            // notable env controls that we don't care too much about:
            // 1: common for all flows, 00020001 = activate, 00080004 = deactivate
            // 3: common for all flows, happens a bit after cast start, always 00010001
            if (index == 2)
            {
                // flow rotation arrows (note that we could also rely on cast id for them...)
                if (state == 0x00020001)
                    _flow = FlowDirection.CW;
                else if (state == 0x00200010)
                    _flow = FlowDirection.CCW;
                // other states: 00080004, 00400004 - deactivation
            }
            else if (index == 5)
            {
                switch (state)
                {
                    case 0x00020001:
                        _fireLine.Add(_module.Arena.WorldNE);
                        _fireLine.Add(_module.Arena.WorldSW);
                        break;
                    case 0x00400020:
                        _fireLine.Add(_module.Arena.WorldNW);
                        _fireLine.Add(_module.Arena.WorldSE);
                        break;
                }
            }
            else if (index >= 9 && index <= 24 && state == 0x00200010)
            {
                // birds, behemoths and snakes; other states: 20001000 = color change, 40000004 = disappear
                switch (index)
                {
                    case  9: _behemoths.Add(_module.Arena.WorldCenter + new Vector3(-_birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 10: _behemoths.Add(_module.Arena.WorldCenter + new Vector3( _birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 11: _behemoths.Add(_module.Arena.WorldCenter + new Vector3(-_birdBehemothOffset, 0,  _birdBehemothOffset)); break;
                    case 12: _behemoths.Add(_module.Arena.WorldCenter + new Vector3( _birdBehemothOffset, 0,  _birdBehemothOffset)); break;
                    case 13:
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeFarOffset,  0, _module.Arena.WorldCenter.Z - _snakeOrthoOffset, 0));
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeNearOffset, 0, _module.Arena.WorldCenter.Z - _snakeOrthoOffset, 0));
                        break;
                    case 14:
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeNearOffset, 0, _module.Arena.WorldCenter.Z - _snakeOrthoOffset, 0));
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeFarOffset,  0, _module.Arena.WorldCenter.Z - _snakeOrthoOffset, 0));
                        break;
                    case 15:
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeFarOffset,  0, _module.Arena.WorldCenter.Z + _snakeOrthoOffset, MathF.PI));
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeNearOffset, 0, _module.Arena.WorldCenter.Z + _snakeOrthoOffset, MathF.PI));
                        break;
                    case 16:
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeNearOffset, 0, _module.Arena.WorldCenter.Z + _snakeOrthoOffset, MathF.PI));
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeFarOffset,  0, _module.Arena.WorldCenter.Z + _snakeOrthoOffset, MathF.PI));
                        break;
                    case 17:
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z - _snakeFarOffset,  MathF.PI / 2));
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z + _snakeNearOffset, MathF.PI / 2));
                        break;
                    case 18:
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z - _snakeNearOffset, MathF.PI / 2));
                        _snakes.Add(new(_module.Arena.WorldCenter.X - _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z + _snakeFarOffset,  MathF.PI / 2));
                        break;
                    case 19:
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z - _snakeFarOffset,  -MathF.PI / 2));
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z + _snakeNearOffset, -MathF.PI / 2));
                        break;
                    case 20:
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z - _snakeNearOffset, -MathF.PI / 2));
                        _snakes.Add(new(_module.Arena.WorldCenter.X + _snakeOrthoOffset, 0, _module.Arena.WorldCenter.Z + _snakeFarOffset,  -MathF.PI / 2));
                        break;
                    case 21: _birds.Add(_module.Arena.WorldCenter + new Vector3(-_birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 22: _birds.Add(_module.Arena.WorldCenter + new Vector3( _birdBehemothOffset, 0, -_birdBehemothOffset)); break;
                    case 23: _birds.Add(_module.Arena.WorldCenter + new Vector3(-_birdBehemothOffset, 0,  _birdBehemothOffset)); break;
                    case 24: _birds.Add(_module.Arena.WorldCenter + new Vector3( _birdBehemothOffset, 0,  _birdBehemothOffset)); break;
                }
            }
        }

        private Vector3 RotatedPosition(Vector3 pos)
        {
            return _flow switch
            {
                FlowDirection.CW => new(-pos.Z, 0, pos.X),
                FlowDirection.CCW => new(pos.Z, 0, -pos.X),
                _ => pos
            };
        }

        private Vector4 RotatedPosRot(Vector4 posRot)
        {
            return _flow switch
            {
                FlowDirection.CW => new(-posRot.Z, 0, posRot.X, posRot.W - MathF.PI / 2),
                FlowDirection.CCW => new(posRot.Z, 0, -posRot.X, posRot.W + MathF.PI / 2),
                _ => posRot
            };
        }
    }
}
