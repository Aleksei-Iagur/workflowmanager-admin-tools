﻿//Copyright 2015 Esri
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//    http://www.apache.org/licenses/LICENSE-2.0
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.​

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.JTX;


namespace WorkflowManagerAdministrationUtilities
{
    class CreateSpatialNotification : WmauAbstractGpFunction
    {
        #region Constants
        private const string C_PARAM_NAME = "in_string_name";
        private const string C_PARAM_EMAIL_NOTIFIER = "in_string_emailNotifier";
        private const string C_PARAM_DESCRIPTION = "in_string_description";
        private const string C_PARAM_SUMMARIZE = "in_bool_summarize";
        private const string C_PARAM_OUT_NAME = "out_string_name";

        private const string C_OPT_SUMMARIZE = "SUMMARIZE";
        private const string C_OPT_DO_NOT_SUMMARIZE = "DO_NOT_SUMMARIZE";

        private const bool C_DEFAULT_SUMMARIZE = false;

        // These values match up with other predetermined values referenced by Workflow
        // Manager.  Do not modify.
        private const string C_PROP_SUBJECT = "Subject";
        private const string C_PROP_SENDER_EMAIL = "SenderEmail";
        private const string C_PROP_SENDER_DISPLAY = "SenderDisplay";
        private const string C_PROP_BODY = "Body";
        private const string C_PROP_SUBSCRIBERS = "Subscribers";

        private const string C_TYPE_EMAIL_NOTIFIER = "Email Notifier";
        #endregion

        #region MemberVariables
        private string m_snName = string.Empty;
        private string m_emailNotifier = string.Empty;
        private string m_snDescription = string.Empty;
        private bool m_summarize = C_DEFAULT_SUMMARIZE;
        #endregion

        #region SimpleAccessors
        public override string Name { get { return "CreateSpatialNotificationWithEmailNotifier"; } }
        public override string DisplayName { get { return Properties.Resources.TOOL_CREATE_SPATIAL_NOTIFICATION_WITH_EMAIL; } }
        public override string DisplayToolset { get { return Properties.Resources.CAT_NOTIFICATION_UTILS; } }
        #endregion

        #region Private helper functions
        /// <summary>
        /// Updates the internal values used by this tool based on the parameters from an input array
        /// </summary>
        /// <param name="paramValues"></param>
        protected override void ExtractParameters(IArray paramValues)
        {
            // Get the values for any parameters common to all GP tools
            ExtractParametersCommon(paramValues);

            WmauParameterMap paramMap = new WmauParameterMap(paramValues);
            IGPParameter3 param = null;

            // Update the internal values of whatever parameters we're maintaining
            param = paramMap.GetParam(C_PARAM_NAME);
            m_snName = param.Value.GetAsText();

            param = paramMap.GetParam(C_PARAM_EMAIL_NOTIFIER);
            m_emailNotifier = param.Value.GetAsText();

            param = paramMap.GetParam(C_PARAM_DESCRIPTION);
            m_snDescription = param.Value.GetAsText();

            param = paramMap.GetParam(C_PARAM_SUMMARIZE);
            m_summarize = bool.Parse(param.Value.GetAsText());
        }

        /// <summary>
        /// Creates a new spatial notifier object based on the specified type.
        /// </summary>
        /// <param name="name">
        /// The type of notifier object to create.  Note that this is NOT
        /// the same as a change rule.  (Change rules include spatial notifiers, as the notifiers
        /// are the actual mechanism used to communicate information to a list of subscribers.)
        /// </param>
        /// <returns>
        /// A new spatial notifier object of the given type, or null if the type was
        /// not found.
        /// </returns>
        IJTXSpatialNotifier CreateSpatialNotifierByName(string name)
        {
            IJTXSpatialNotifier retVal = null;

            IJTXSpatialNotificationManager snManager = this.WmxDatabase.SpatialNotificationManager;
            IJTXSpatialNotifierNameSet allSpatialNotifiers = snManager.SpatialNotifiers;
            
            // Loop through all of the spatial notifier name objects ("E-mail notifier", etc.)
            // If/when we find a match, create a spatial notifier object of the correct name/type.
            // (Remember that the name objects are *not* strings!)
            for (int i = 0; i < allSpatialNotifiers.Count; i++)
            {
                IJTXSpatialNotifierName tempNameObj = allSpatialNotifiers.get_Item(i);
                if (tempNameObj.Name.Equals(name))
                {
                    retVal = snManager.CreateSpatialNotifier(tempNameObj);
                    break;
                }
            }

            return retVal;
        }
        #endregion

        /// <summary>
        /// Required by IGPFunction2 interface.
        /// </summary>
        public override IArray ParameterInfo
        {
            get
            {
                m_parameters = new ArrayClass();
                IGPParameterEdit3 paramEdit = null;
                IGPCodedValueDomain cvDomain = null;

                // Spatial notification name
                paramEdit = BuildParameter(
                    esriGPParameterDirection.esriGPParameterDirectionInput,
                    esriGPParameterType.esriGPParameterTypeRequired,
                    Properties.Resources.DESC_CSN_NAME,
                    C_PARAM_NAME,
                    new GPStringTypeClass(),
                    null);
                m_parameters.Add(paramEdit);

                // Name of e-mail notification to use as a template for this notification
                paramEdit = BuildParameter(
                    esriGPParameterDirection.esriGPParameterDirectionInput,
                    esriGPParameterType.esriGPParameterTypeRequired,
                    Properties.Resources.DESC_CSN_EMAIL_NOTIFIER,
                    C_PARAM_EMAIL_NOTIFIER,
                    new GPStringTypeClass(),
                    null,
                    true);
                m_parameters.Add(paramEdit);

                // Description of this notification
                paramEdit = BuildParameter(
                    esriGPParameterDirection.esriGPParameterDirectionInput,
                    esriGPParameterType.esriGPParameterTypeOptional,
                    Properties.Resources.DESC_CSN_DESCRIPTION,
                    C_PARAM_DESCRIPTION,
                    new GPStringTypeClass(),
                    null);
                m_parameters.Add(paramEdit);

                // Summarize the notification results or not
                cvDomain = new GPCodedValueDomainClass();
                cvDomain.AddCode(GpTrue, C_OPT_SUMMARIZE);
                cvDomain.AddCode(GpFalse, C_OPT_DO_NOT_SUMMARIZE);

                paramEdit = BuildParameter(
                    esriGPParameterDirection.esriGPParameterDirectionInput,
                    esriGPParameterType.esriGPParameterTypeOptional,
                    Properties.Resources.DESC_CSN_SUMMARIZE,
                    C_PARAM_SUMMARIZE,
                    GpBooleanType,
                    ToGpBoolean(C_DEFAULT_SUMMARIZE));
                paramEdit.Domain = cvDomain as IGPDomain;
                m_parameters.Add(paramEdit);

                // Parameter for specifying the WMX database
                m_parameters.Add(BuildWmxDbParameter());

                // Spatial notification name
                paramEdit = BuildParameter(
                    esriGPParameterDirection.esriGPParameterDirectionOutput,
                    esriGPParameterType.esriGPParameterTypeDerived,
                    Properties.Resources.DESC_CSN_OUT_NAME,
                    C_PARAM_OUT_NAME,
                    new GPStringTypeClass(),
                    null);
                m_parameters.Add(paramEdit);

                return m_parameters;
            }
        }

        /// <summary>
        /// Post validates the given set of values.
        /// This is where you flag parameters with warnings and error messages, among other things.
        /// </summary>
        /// <param name="paramValues"></param>
        /// <param name="pEnvMgr"></param>
        /// <param name="msgs"></param>
        public override void UpdateMessages(IArray paramValues, IGPEnvironmentManager pEnvMgr, IGPMessages msgs)
        {
            // Call the base class function first
            try
            {
                UpdateMessagesCommon(paramValues, pEnvMgr, msgs);
            }
            catch (WmxDefaultDbNotSetException)
            {
                // If the default DB wasn't set, stop executing
                return;
            }

            // Build a hash of which parameter is at which index for ease of access
            WmauParameterMap paramMap = new WmauParameterMap(paramValues);

            IGPParameter3 snNameParam = paramMap.GetParam(C_PARAM_NAME);
            IGPParameterEdit3 snNameParamEdit = paramMap.GetParamEdit(C_PARAM_NAME);

            // Ensure that the named spatial notification doesn't already exist
            if (snNameParam.Value != null && !snNameParam.Value.GetAsText().Equals(string.Empty))
            {
                IJTXDatabase2 db = this.WmxDatabase as IJTXDatabase2;
                string changeRuleName = snNameParam.Value.GetAsText();
                IJTXChangeRuleSet allChangeRules = db.SpatialNotificationManager.ChangeRules;
                for (int i = 0; i < allChangeRules.Count; i++)
                {
                    IJTXChangeRule tempRule = allChangeRules.get_Item(i);
                    if (tempRule.Name.Equals(snNameParam.Value.GetAsText()))
                    {
                        WmauError error = new WmauError(WmauErrorCodes.C_SN_EXISTS_ERROR);
                        msgs.ReplaceError(paramMap.GetIndex(C_PARAM_NAME), error.ErrorCodeAsInt, error.Message);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Pre validates the given set of values.
        /// This is where you populate derived parameters based on input, among other things.
        /// </summary>
        /// <param name="paramValues"></param>
        /// <param name="pEnvMgr"></param>
        public override void UpdateParameters(IArray paramValues, IGPEnvironmentManager pEnvMgr)
        {
            try
            {
                UpdateParametersCommon(paramValues, pEnvMgr);
            }
            catch (WmxDefaultDbNotSetException)
            {
                // If the default DB wasn't set, stop executing
                return;
            }
            catch (NullReferenceException)
            {
                // If one of the parameters was null, stop executing
                return;
            }

            // Build a hash of which parameter is at which index for ease of access
            WmauParameterMap paramMap = new WmauParameterMap(paramValues);

            IGPParameter3 notificationParam = paramMap.GetParam(C_PARAM_EMAIL_NOTIFIER);
            IGPParameterEdit3 notificationParamEdit = paramMap.GetParamEdit(C_PARAM_EMAIL_NOTIFIER);

            // Add a domain to the notification parameter
            if (notificationParam.Domain == null || (notificationParam.Domain as IGPCodedValueDomain).CodeCount <= 0)
            {
                notificationParamEdit.Domain = Common.WmauGpDomainBuilder.BuildEmailNotificationDomain(this.WmxDatabase);
            }
        }

        /// <summary>
        /// Required by IGPFunction2 interface; this function is called when the GP tool is ready to be executed.
        /// </summary>
        /// <param name="paramValues"></param>
        /// <param name="trackCancel"></param>
        /// <param name="envMgr"></param>
        /// <param name="msgs"></param>
        public override void Execute(IArray paramValues, ITrackCancel trackCancel, IGPEnvironmentManager envMgr, IGPMessages msgs)
        {
            // Do some common error-checking
            base.Execute(paramValues, trackCancel, envMgr, msgs);

            try
            {
                // Ensure that the current user has admin access to the current Workflow Manager DB
                if (!CurrentUserIsWmxAdministrator())
                {
                    throw new WmauException(WmauErrorCodes.C_USER_NOT_ADMIN_ERROR);
                }

                IJTXSpatialNotificationManager snManager = this.WmxDatabase.SpatialNotificationManager;
                IJTXSpatialNotifierNameSet allSnNames = snManager.SpatialNotifiers;
                IJTXNotificationConfiguration notificationConfig = this.WmxDatabase.ConfigurationManager as IJTXNotificationConfiguration;

                // Create a new spatial notification
                IJTXChangeRule2 changeRule = snManager.AddChangeRule() as IJTXChangeRule2;

                // Set the name
                changeRule.Name = m_snName;

                // Set the notifier
                IJTXNotificationType srcNotifType = notificationConfig.GetNotificationType(m_emailNotifier);

                // Set the properties of the spatial notification's e-mail notification to match that
                // of the source e-mail notification (phew!)
                IJTXEmailSpatialNotifier emailNotifier = this.CreateSpatialNotifierByName(C_TYPE_EMAIL_NOTIFIER) as IJTXEmailSpatialNotifier;
                emailNotifier.Subject = srcNotifType.SubjectTemplate;
                emailNotifier.Body = srcNotifType.MessageTemplate;
                emailNotifier.SenderEmail = srcNotifType.SenderTemplate;
                emailNotifier.SenderDisplayName = srcNotifType.SenderDisplayNameTemplate;

                string[] subscribers = srcNotifType.get_Subscribers();
                IStringArray subscribersObj = new StrArrayClass();
                foreach (string subscriber in subscribers)
                {
                    subscribersObj.Add(subscriber);
                }
                emailNotifier.Subscribers = subscribersObj;

                changeRule.Notifier = emailNotifier as IJTXSpatialNotifier;

                // Set the description, if applicable
                if (!string.IsNullOrEmpty(m_snDescription))
                {
                    changeRule.Description = m_snDescription;
                }

                // Set the summarization behavior
                changeRule.SummarizeNotifications = m_summarize;

                // Store the resulting change rule
                changeRule.Store();

                // Update the output parameter
                WmauParameterMap paramMap = new WmauParameterMap(paramValues);
                IGPParameterEdit3 outParam = paramMap.GetParamEdit(C_PARAM_OUT_NAME);
                IGPString strValue = new GPStringClass();
                strValue.Value = m_snName;
                outParam.Value = strValue as IGPValue;

                msgs.AddWarning("To avoid database corruption, at least one dataset or area evaluator must be added to notification '" + m_snName + "' immediately!");
                msgs.AddMessage(Properties.Resources.MSG_DONE);
            }
            catch (WmauException wmEx)
            {
                try
                {
                    msgs.AddError(wmEx.ErrorCodeAsInt, wmEx.Message);
                }
                catch
                {
                    // Catch anything else that possibly happens
                }
            }
            catch (Exception ex)
            {
                try
                {
                    WmauError error = new WmauError(WmauErrorCodes.C_SN_CREATION_ERROR);
                    msgs.AddError(error.ErrorCodeAsInt, error.Message + "; " + ex.Message);
                }
                catch
                {
                    // Catch anything else that possibly happens
                }
            }
            finally
            {
                // Release any COM objects here!
            }
        }
    }
}
