﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupScheduleToolbox.ascx.cs" Inherits="RockWeb.Blocks.GroupScheduling.GroupScheduleToolbox" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfSelectedPersonId" runat="server" />
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-calendar"></i>
                    <asp:Literal ID="lTitle" runat="server" Text="Schedule Toolbox" />
                </h1>

                <div class="panel-labels">
                </div>
            </div>

            <asp:Panel ID="pnlToolbox" CssClass="panel-body" runat="server">

                <div class="margin-b-md">
                    <%--<Rock:ButtonGroup ID="bgTabs" runat="server" SelectedItemClass="btn btn-primary active" UnselectedItemClass="btn btn-default" AutoPostBack="true" OnSelectedIndexChanged="bgTabs_SelectedIndexChanged" />--%>
                    <ul class="nav nav-pills margin-b-md">
                        <asp:Repeater ID="rptTabs" runat="server">
                            <ItemTemplate>
                                <li class='<%# GetTabClass(Container.DataItem) %>'>
                                    <asp:LinkButton ID="lbTab" runat="server" Text='<%# GetTabName(Container.DataItem) %>' CommandArgument="<%# Container.DataItem %>" OnClick="lbTab_Click" CausesValidation="false">
                                    </asp:LinkButton>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </div>

                <%-- My Schedule --%>
                <asp:Panel ID="pnlMySchedule" runat="server">

                    <Rock:NotificationBox ID="nbNoUpcomingSchedules" runat="server" Visible="false" Text="No upcoming schedules" NotificationBoxType="Info" />

                    <%-- Pending Confirmations Grid --%>
                    <asp:Panel ID="pnlPendingConfirmations" runat="server" CssClass="pending-confirmations margin-b-lg">
                        <span class="control-label">
                            <asp:Literal runat="server" ID="lPendingConfirmations" Text="Pending Confirmations" />
                        </span>
                        <table class="table table-borderless">
                            <tbody>
                                <asp:Repeater ID="rptPendingConfirmations" runat="server" OnItemDataBound="rptPendingConfirmations_ItemDataBound">
                                    <ItemTemplate>
                                        <tr>
                                            <td>
                                                <asp:Literal ID="lPendingOccurrenceDetails" runat="server" />
                                            </td>
                                            <td>
                                                <asp:Literal ID="lPendingOccurrenceTime" runat="server" />

                                            </td>
                                            <td>
                                                <div class="actions">
                                                    <asp:LinkButton ID="btnConfirmAttend" runat="server" CssClass="btn btn-xs btn-success" Text="Attend" OnClick="btnConfirmAttend_Click" />
                                                    <asp:LinkButton ID="btnDeclineAttend" runat="server" CssClass="btn btn-xs btn-danger" Text="Decline" OnClick="btnDeclineAttend_Click" />
                                                </div>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </asp:Panel>

                    <%-- Upcoming Schedules Grid --%>
                    <asp:Panel ID="pnlUpcomingSchedules" runat="server" CssClass="confirmed margin-t-md">
                        <span class="control-label">
                            <asp:Literal runat="server" ID="lUpcomingSchedules" Text="Upcoming Schedules" />
                            <button id="btnCopyToClipboard" runat="server" disabled="disabled"
                                data-toggle="tooltip" data-placement="top" data-trigger="hover" data-delay="250" title="Copies the link to synchronize your schedule with a calendar such as Microsoft Outlook or Google Calendar"
                                class="btn btn-info btn-xs btn-copy-to-clipboard margin-l-md margin-b-sm"
                                onclick="$(this).attr('data-original-title', 'Copied').tooltip('show').attr('data-original-title', 'Copy Link to Clipboard');return false;">
                                <i class="fa fa-calendar-alt"></i> Copy Calendar Link
                            </button>
                        </span>
                        <table class="table table-borderless">
                            <tbody>
                                <asp:Repeater ID="rptUpcomingSchedules" runat="server" OnItemDataBound="rptUpcomingSchedules_ItemDataBound">
                                    <ItemTemplate>
                                        <tr>
                                            <td>
                                                <asp:Literal ID="lConfirmedOccurrenceDetails" runat="server" />
                                            </td>
                                            <td>
                                                <asp:Literal ID="lConfirmedOccurrenceTime" runat="server" />
                                            </td>
                                            <td>
                                                <asp:LinkButton ID="btnCancelConfirmAttend" runat="server" CssClass="btn btn-xs btn-link text-danger" Text="Cancel Confirmation" OnClick="btnDeclineAttend_Click" />
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </asp:Panel>

                </asp:Panel>

                <%-- Preferences --%>
                <asp:Panel ID="pnlPreferences" runat="server">
                    <div class="row">
                        <div class="col-md-6">

                            <Rock:NotificationBox ID="nbNoScheduledGroups" runat="server" Visible="false" Text="You are currently not in any scheduled groups." NotificationBoxType="Info" />

                            <%-- Per Group Preferences --%>
                            <asp:Repeater ID="rptGroupPreferences" runat="server" OnItemDataBound="rptGroupPreferences_ItemDataBound">
                                <ItemTemplate>
                                    <asp:HiddenField ID="hfPreferencesGroupId" runat="server" />

                                    <h3>
                                        <asp:Literal runat="server" ID="lGroupPreferencesGroupNameHtml" /></h3>
                                    <hr class="margin-t-sm margin-b-sm" />

                                    <div class="row">
                                        <div class="col-md-6">
                                            <Rock:RockDropDownList ID="ddlSendRemindersDaysOffset" runat="server" Label="Send Reminders" OnSelectedIndexChanged="ddlSendRemindersDaysOffset_SelectedIndexChanged" AutoPostBack="true">
                                                <asp:ListItem Value=""></asp:ListItem>
                                                <asp:ListItem Value="-1" Text="Do not send a reminder"></asp:ListItem>
                                                <asp:ListItem Value="1" Text="1 day before"></asp:ListItem>
                                                <asp:ListItem Value="2" Text="2 days before"></asp:ListItem>
                                                <asp:ListItem Value="3" Text="3 days before"></asp:ListItem>
                                                <asp:ListItem Value="4" Text="4 days before"></asp:ListItem>
                                                <asp:ListItem Value="5" Text="5 days before"></asp:ListItem>
                                                <asp:ListItem Value="6" Text="6 days before"></asp:ListItem>
                                                <asp:ListItem Value="7" Text="7 days before"></asp:ListItem>
                                                <asp:ListItem Value="8" Text="8 days before"></asp:ListItem>
                                                <asp:ListItem Value="9" Text="9 days before"></asp:ListItem>
                                                <asp:ListItem Value="10" Text="10 days before"></asp:ListItem>
                                                <asp:ListItem Value="11" Text="11 days before"></asp:ListItem>
                                                <asp:ListItem Value="12" Text="12 days before"></asp:ListItem>
                                                <asp:ListItem Value="13" Text="13 days before"></asp:ListItem>
                                                <asp:ListItem Value="14" Text="14 days before"></asp:ListItem>
                                            </Rock:RockDropDownList>

                                        </div>
                                        <div class="col-md-6">
                                        </div>

                                    </div>

                                    <div class="row">
                                        <div class="col-md-6">
                                            <Rock:RockDropDownList ID="ddlGroupMemberScheduleTemplate" runat="server" Label="Current Schedule" OnSelectedIndexChanged="ddlGroupMemberScheduleTemplate_SelectedIndexChanged" AutoPostBack="true" />
                                        </div>
                                        <div class="col-md-6">
                                            <Rock:DatePicker ID="dpGroupMemberScheduleTemplateStartDate" runat="server" Label="Starting On" OnValueChanged="dpGroupMemberScheduleTemplateStartDate_ValueChanged" />
                                        </div>
                                    </div>

                                    <asp:Panel ID="pnlGroupPreferenceAssignment" runat="server" Visible="false">

                                        <span class="control-label">
                                            <asp:Literal runat="server" ID="lGroupPreferenceAssignmentLabel" Text="Assignment" />
                                        </span>
                                        <p>
                                            <asp:Literal runat="server" ID="lGroupPreferenceAssignmentHelp" Text="Please select a time and optional location that you would like to be scheduled for." />
                                        </p>

                                        <%-- NOTE: This gGroupPreferenceAssignments (and these other controls in the ItemTemplate) is in a repeater and is configured in rptGroupPreferences_ItemDataBound--%>
                                        <Rock:Grid ID="gGroupPreferenceAssignments" runat="server" DisplayType="Light" OnRowDataBound="gGroupPreferenceAssignments_RowDataBound" RowItemText="Group Preference Assignment" AllowPaging="false">
                                            <Columns>
                                                <Rock:RockLiteralField ID="lScheduleName" HeaderText="Schedule" />
                                                <Rock:RockLiteralField ID="lLocationName" HeaderText="Location" />
                                                <Rock:LinkButtonField ID="btnEditGroupPreferenceAssignment" CssClass="btn btn-default btn-sm" Text="<i class='fa fa-pencil'></i>" OnClick="btnEditGroupPreferenceAssignment_Click" />
                                                <Rock:DeleteField OnClick="btnDeleteGroupPreferenceAssignment_Click" />
                                            </Columns>
                                        </Rock:Grid>

                                        <br />
                                    </asp:Panel>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <%-- Blackout Dates --%>
                        <asp:Panel ID="pnlBlackoutDates" runat="server" CssClass="col-md-6">
                            <div class="well">
                                <h3>
                                    <asp:Literal runat="server" ID="lBlackoutDates" Text="Blackout Dates" />
                                </h3>
                                <hr class="margin-t-sm margin-b-sm" />
                                <p>
                                    Please provide any dates <%= ( CurrentPersonId == null || CurrentPersonId != SelectedPersonId ? "they" : "you") %> will not be able to attend.
                                </p>

                                <Rock:Grid ID="gBlackoutDates" CssClass="bg-transparent" runat="server" EmptyDataText="No black out dates have been set." DataKeyNames="ExclusionId" ShowHeader="false" DisplayType="Light">
                                    <Columns>
                                        <Rock:RockBoundField DataField="ExclusionId" Visible="false"></Rock:RockBoundField>
                                        <Rock:RockBoundField DataField="PersonAliasId" Visible="false"></Rock:RockBoundField>
                                        <Rock:RockTemplateField>
                                            <ItemTemplate>
                                                <asp:Literal ID="litExclusionDateRange" runat="server" Text='<%# Eval("DateRange")%>'></asp:Literal><span> - </span>
                                                <asp:Literal ID="litExclusionFullName" runat="server" Text='<%# Eval("FullName") %>'></asp:Literal><span> - </span>
                                                <asp:Literal ID="litExclusionGroupName" runat="server" Text='<%# Eval("GroupName") %>'></asp:Literal>
                                            </ItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:DeleteField ID="gBlackoutDatesDelete" runat="server" OnClick="gBlackoutDatesDelete_Click"></Rock:DeleteField>
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </asp:Panel>
                    </div>
                </asp:Panel>

                <%-- Preferences Add/Edit GroupScheduleAssignment modal --%>
                <Rock:ModalDialog ID="mdGroupScheduleAssignment" runat="server" OnSaveClick="mdGroupScheduleAssignment_SaveClick" Title="Add/Edit Assignment" >
                    <Content>
                        <asp:HiddenField ID="hfGroupScheduleAssignmentGroupId" runat="server" />
                        <asp:HiddenField ID="hfGroupScheduleAssignmentId" runat="server" />
                        <Rock:RockDropDownList ID="ddlGroupScheduleAssignmentSchedule" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlGroupScheduleAssignmentSchedule_SelectedIndexChanged" Label="Schedule" Required="true" />
                        <Rock:RockDropDownList ID="ddlGroupScheduleAssignmentLocation" runat="server" Label="Location" />
                    </Content>
                </Rock:ModalDialog>

                <%-- Sign-up --%>
                <asp:Panel ID="pnlSignup" CssClass="row" runat="server">
                    <div class="col-md-12">
                        <asp:Literal ID="lSignupMsg" runat="server" />
                        <Rock:DynamicPlaceholder ID="phSignUpSchedules" runat="server" />
                    </div>
                </asp:Panel>

            </asp:Panel>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <%-- BlackoutDates Modal  --%>
        <Rock:ModalDialog ID="mdAddBlackoutDates" runat="server" Title="Add Blackout Dates" OnSaveClick="mdAddBlackoutDates_SaveClick" OnCancelScript="clearActiveDialog();" ValidationGroup="AddBlackOutDates">
            <Content>
                <p>
                    <label>Choose the dates, group, and people who will be unavailable</label>
                </p>
                <asp:ValidationSummary ID="valSummaryAddBlackoutDates" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="AddBlackOutDates" />

                <Rock:DateRangePicker ID="drpBlackoutDateRange" runat="server" Label="Date Range" ValidationGroup="AddBlackOutDates" Required="true" RequiredErrorMessage="Date Range is required" />
                <Rock:RockTextBox ID="tbBlackoutDateDescription" runat="server" Label="Description" MaxLength="100" Help="A short description of why you'll be unavailable" />

                <Rock:RockDropDownList ID="ddlBlackoutGroups" runat="server" Label="Group" />
                <Rock:RockCheckBoxList ID="cblBlackoutPersons" runat="server" RepeatDirection="Vertical" RepeatColumns="1" Label="Individual" ValidationGroup="AddBlackOutDates" Required="true" RequiredErrorMessage="At least one person must be selected" />

            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            function clearActiveDialog() {
                $('#<%=hfActiveDialog.ClientID %>').val('');
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>
