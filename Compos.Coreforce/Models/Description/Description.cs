﻿using System.Collections.Generic;

namespace Compos.Coreforce.Models.Description
{
    public class Description
    {
        public List<ActionOverrideDescription> actionOverrides { get; set; }
        public bool activateable { get; set; }
        public List<ChildRelationshipDescription> childRelationships { get; set; }
        public bool compactLayoutable { get; set; }
        public bool createable { get; set; }
        public bool custom { get; set; }
        public bool customSetting { get; set; }
        public bool deletable { get; set; }
        public bool deprecatedAndHidden { get; set; }
        public bool feedEnabled { get; set; }
        public List<FieldDescription> fields { get; set; }
        public string keyPrefix { get; set; }
        public string label { get; set; }
        public string labelPlural { get; set; }
        public bool layoutable { get; set; }
        public object listviewable { get; set; }
        public object lookupLayoutable { get; set; }
        public bool mergeable { get; set; }
        public bool mruEnabled { get; set; }
        public string name { get; set; }
        public List<object> namedLayoutInfos { get; set; }
        public object networkScopeFieldName { get; set; }
        public bool queryable { get; set; }
        public List<RecordTypeInfoDescription> recordTypeInfos { get; set; }
        public bool replicateable { get; set; }
        public bool retrieveable { get; set; }
        public bool searchLayoutable { get; set; }
        public bool searchable { get; set; }
        public List<SupportedScopeDescription> supportedScopes { get; set; }
        public bool triggerable { get; set; }
        public bool undeletable { get; set; }
        public bool updateable { get; set; }
        public UrlsDescription urls { get; set; }
    }
}
