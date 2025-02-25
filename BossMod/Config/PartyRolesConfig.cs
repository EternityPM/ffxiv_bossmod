﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod
{
    [ConfigDisplay(Name = "小队职责分配", Order = 2)]
    public class PartyRolesConfig : ConfigNode
    {
        public enum Assignment { MT, ST, H1, H2, D1, D2, D3, D4, Unassigned }

        public Dictionary<ulong, Assignment> Assignments = new();

        public Assignment this[ulong contentID] => Assignments.GetValueOrDefault(contentID, Assignment.Unassigned);

        // return either array of assigned roles per party slot (if each role is assigned exactly once) or empty array (if assignments are invalid)
        public Assignment[] AssignmentsPerSlot(PartyState party)
        {
            int[] counts = new int[(int)Assignment.Unassigned];
            Assignment[] res = new Assignment[PartyState.MaxPartySize];
            Array.Fill(res, Assignment.Unassigned);
            for (int i = 0; i < PartyState.MaxPartySize; ++i)
            {
                var r = this[party.ContentIDs[i]];
                if (r == Assignment.Unassigned)
                    return new Assignment[0];
                if (counts[(int)r]++ > 0)
                    return new Assignment[0];
                res[i] = r;
            }
            return res;
        }

        // return either array of party slots per assigned role (if each role is assigned exactly once) or empty array (if assignments are invalid)
        public int[] SlotsPerAssignment(PartyState party)
        {
            int[] res = new int[(int)Assignment.Unassigned];
            Array.Fill(res, PartyState.MaxPartySize);
            for (int i = 0; i < PartyState.MaxPartySize; ++i)
            {
                var r = this[party.ContentIDs[i]];
                if (r == Assignment.Unassigned)
                    return new int[0];
                if (res[(int)r] != PartyState.MaxPartySize)
                    return new int[0];
                res[(int)r] = i;
            }
            return res;
        }

        // return array of effective roles per party slot
        public Role[] EffectiveRolePerSlot(PartyState party)
        {
            var res = new Role[PartyState.MaxPartySize];
            for (int i = 0; i < PartyState.MaxPartySize; ++i)
            {
                res[i] = this[party.ContentIDs[i]] switch
                {
                    Assignment.MT or Assignment.ST => Role.Tank,
                    Assignment.H1 or Assignment.H2 => Role.Healer,
                    Assignment.D1 or Assignment.D2 => Role.Melee,
                    Assignment.D3 or Assignment.D4 => Role.Ranged,
                    _ => party[i]?.Role ?? Role.None
                };
            }
            return res;
        }

        public override void DrawCustom(UITree tree, WorldState ws)
        {
            if (ImGui.BeginTable("tab2", 10, ImGuiTableFlags.SizingFixedFit))
            {
                foreach (var r in typeof(Assignment).GetEnumValues())
                    ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                ImGui.TableSetupColumn("名称");
                ImGui.TableHeadersRow();

                List<(ulong cid, string name, Role role, Assignment assignment)> party = new();
                for (int i = 0; i < PartyState.MaxPartySize; ++i)
                {
                    var m = ws.Party.Members[i];
                    if (m != null)
                        party.Add((ws.Party.ContentIDs[i], m.Name, m.Role, this[ws.Party.ContentIDs[i]]));
                }
                party.SortBy(e => e.role);

                foreach (var (contentID, name, classRole, assignment) in party)
                {
                    ImGui.TableNextRow();
                    foreach (var r in typeof(Assignment).GetEnumValues().Cast<Assignment>())
                    {
                        ImGui.TableNextColumn();
                        if (ImGui.RadioButton($"###{contentID:X}:{r}", assignment == r))
                        {
                            if (r != Assignment.Unassigned)
                                Assignments[contentID] = r;
                            else
                                Assignments.Remove(contentID);
                            NotifyModified();
                        }
                    }
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"({classRole.ToString()[0]}) {name}");
                }
                ImGui.EndTable();

                if (AssignmentsPerSlot(ws.Party).Length == 0)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ffff);
                    ImGui.TextUnformatted("无效分配: 每一个职责都必须有一个玩家!");
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xff00ff00);
                    ImGui.TextUnformatted("没有问题!");
                    ImGui.PopStyleColor();
                }
            }
        }
    }
}
