﻿using ImGuiNET;
using System;
using System.Linq;

namespace BossMod
{
    public class CooldownPlanEditor
    {
        private Action _onModified;
        private Timeline _timeline = new();
        private ColumnStateMachineBranch _colStates;
        private CooldownPlannerColumns _planner;
        private int _selectedPhase = 0;
        private bool _modified = false;

        public CooldownPlanEditor(CooldownPlan plan, StateMachine sm, ModuleRegistry.Info? moduleInfo, Action onModified)
        {
            _onModified = onModified;

            var tree = new StateMachineTree(sm);
            var phaseBranches = Enumerable.Repeat(0, tree.Phases.Count).ToList();
            _colStates = _timeline.Columns.Add(new ColumnStateMachineBranch(_timeline, tree, phaseBranches));
            _planner = _timeline.Columns.Add(new CooldownPlannerColumns(plan, OnPlanModified, _timeline, tree, phaseBranches, moduleInfo, true));

            _timeline.MinTime = -30;
            _timeline.MaxTime = tree.TotalMaxTime;
        }

        public void Draw()
        {
            if (ImGui.Button(_modified ? "保存" : "无更改") && _modified)
                Save();
            ImGui.SameLine();
            _planner.DrawCommonControls();

            _selectedPhase = _planner.DrawPhaseControls(_selectedPhase);

            _timeline.Draw();
        }

        private void Save()
        {
            _planner.UpdateEditedPlan();
            _onModified();
            _modified = false;
        }

        private void OnPlanModified()
        {
            _timeline.MaxTime = _colStates.Tree.TotalMaxTime;
            _modified = true;
        }
    }
}
