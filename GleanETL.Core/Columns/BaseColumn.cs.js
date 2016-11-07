var server = server || {};
/// <summary>The BaseColumn class as defined in Glean.Core.Columns.BaseColumn</summary>
server.BaseColumn = function() {
	/// <field name="columnDisplayName" type="String">The ColumnDisplayName property as defined in Glean.Core.Columns.BaseColumn</field>
	this.columnDisplayName = '';
	/// <field name="columnName" type="String">The ColumnName property as defined in Glean.Core.Columns.BaseColumn</field>
	this.columnName = '';
	/// <field name="dataType" type="Object">The DataType property as defined in Glean.Core.Columns.BaseColumn</field>
	this.dataType = { };
	/// <field name="preParseFunction" type="Object">The PreParseFunction property as defined in Glean.Core.Columns.BaseColumn</field>
	this.preParseFunction = { };
};

