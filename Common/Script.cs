using System;
using System.Text.RegularExpressions;

namespace Skyever.Report
{
	/// <summary>
	/// 用于保存一些客户端脚本，并实现脚本的转换等工作
	/// </summary>
	internal abstract class Script
	{
		#region 客户端对汇总屏的操作的一些脚本

		/// <summary>
		/// 对汇总屏脚本的访问
		/// </summary>
		static public string ReportCellOperateScript
		{
			get
			{
				return _ReportCellOperateScript.Script;
			}
		}

		static private ScriptOperate _ReportCellOperateScript = new ScriptOperate(
@"
function ReportCellOperate(clientID)
{
	/*上一页*/
	this.PreviousPage = function(message)
	{
		var curPageIndex = this.GetCurPageIndex();
		if(curPageIndex <= 1)	return;
		this.GotoPage(--curPageIndex, message);
	};
	/*下一页*/
	this.NextPage = function(message)
	{
		var curPageIndex = this.GetCurPageIndex();
		if(curPageIndex > this.GetPageCount())	return;
		this.GotoPage(++curPageIndex, message);
	};
	/*第一页*/
	this.FirstPage = function(message)
	{
		var curPageIndex = this.GetCurPageIndex();
		if(curPageIndex == 1)	return;
		this.GotoPage(1, message);
	};
	/*最后一页*/
	this.LastPage = function(message)
	{
		var curPageIndex = this.GetCurPageIndex();
		if(curPageIndex == this.GetPageCount())	return;
		this.GotoPage(this.GetPageCount(), message);
	};
	/*刷新*/
	this.Refresh = function(message)
	{
		if(message==true)	message = '即将刷新屏幕，是否继续？';
		this.SendMessage('Refresh', message);
	};
	/*转到第几页*/
	this.GotoPage = function(index, message)
	{
		var curPageIndex = this.GetCurPageIndex();
		if(index==curPageIndex)			return;
		if(index<=0)					return;
		if(index>this.GetPageCount())	return;
		document.all(clientID + '_CurPageIndex').value = index;
		if(message == true)		message = '即将翻页，是否继续？';
		this.SendMessage('ChangePage', null, message);
	};
	/*得到总页数*/
	this.GetPageCount = function()
	{
		return parseInt(document.all(clientID + '_PageCount').value);
	};
	/*得到每页条数*/
	this.GetPageVolume = function()
	{
		return parseInt(document.all(clientID + '_PageVolume').value);
	};
	/*得到总条数*/
	this.GetItemCount = function()
	{
		return parseInt(document.all(clientID + '_ItemCount').value);
	};
	/*得到当前页码*/
	this.GetCurPageIndex = function()
	{
		return parseInt(document.all(clientID + '_CurPageIndex').value);
	};
	/*得到Table节点*/
	this.GetTableNode = function()
	{
		return document.all('ReportCell_' + clientID + '_Table');
	};
	/*得到Table的其它节点*/
	this.GetNode = function(tagName)
	{
		if(tagName==null)		return;
		if(tagName=='TABLE')	return this.GetTableNode();
		var node = this.GetTableNode().firstChild;
		while(node!=null)
		{
			if(node.tagName==tagName.toUpperCase())	return node;
			node = node.nextSibling;
		}
		return null;
	};

	/*回发*/
	this.SendMessage = function(command, argument, message)
	{
		if(message==true)	message = '即将执行操作，是否继续？';
		if(message!=null&&message!=false && window.confirm(message)==false)	return;
		document.all(clientID + '_Operate').value = command;
		document.all(clientID + '_Argument').value = argument==null?'':argument;
		document.forms[0].submit();
	};

	/*寻找该汇总屏的指定样式*/
	this.FindStyle = function(name)
	{
		if(name.toLowerCase().replace(RegExp('\\s+', 'img'),'') == 'list:hover')			name = '#reportcell_' + clientID.toLowerCase() + 'tbody.listhover';
		else if(name.toLowerCase().replace(RegExp('\\s+', 'img'),'') == 'list:selected')	name = '#reportcell_' + clientID.toLowerCase() + 'tbody.listselected';
		else	name = ('#reportcell_' + clientID + name).toLowerCase().replace(RegExp('\\s+', 'img'), '');
		var stylesheetname = '#reportcell_' + clientID.toLowerCase();
		for(var ss=0; ss<document.styleSheets.length; ss++)
		{
			var thisStyle = document.styleSheets(ss);
			if(thisStyle.id == null)	continue;
			if(thisStyle.id.toLowerCase() != stylesheetname)	continue;
			for(var sr=0; sr<thisStyle.rules.length; sr++)
			{
				if(thisStyle.rules(sr).selectorText.toLowerCase().replace(RegExp('\\s+', 'img') ,'') == name)	return thisStyle.rules(sr);
			}
		}
		return null;
	};

	/*几个汇总屏的属性*/
	this.IsShowCheckBox = this.GetTableNode().isShowCheckBox.toLowerCase()=='true';
	this.IsMulCheckable = this.GetTableNode().isMulCheckable.toLowerCase()=='true';
	this.IsSelectable	= this.GetTableNode().isSelectable.toLowerCase()=='true';
	this.IsEditable		= this.GetTableNode().isEditable.toLowerCase()=='true';
	this.RowWidth		= this.GetTableNode().rowWidth;

	var curRow=null, bkCurRow=null;
	this._SetCurRow = function() { curRow = bkCurRow; };
	/*选定指定行*/
	this.SelectRow = function(tr, state, isInner)
	{
		var table = this.GetTableNode();
		var tbody = this.GetNode('tbody');
		if(tbody==null)	return;
		
		var isMulCheckable = this.GetTableNode().isMulCheckable.toLowerCase()=='true';
		var isShowCheckBox = this.GetTableNode().isShowCheckBox.toLowerCase()=='true';
		var isExistListSelectedStyle = this.FindStyle('tbody.listselected')!=null;
		if(isInner==true && isShowCheckBox && !isExistListSelectedStyle)	return;		 /*如果没有定义选中样式，并且已显示复选框，则不执行任何操作 */

		if(typeof(tr) == 'number')
		{
			if(tr + tbody.firstChild.rowIndex >= table.rows.length)		return;
			tr = table.rows(tr + tbody.firstChild.rowIndex);
		}
		else if(typeof(tr) == 'string')
		{
			var thistr = tbody.firstChild;
			while(thistr != null)
			{
				if(thistr.flag == tr)	{ tr = thistr; break; }
				thistr = thistr.nextSibling;
			}
			if(thistr == null)	return;
		}
		else if(!(typeof(tr)=='object'&&tr.tagName=='TR'))
		{
			while(tr!=null) { if(tr.tagName=='TR')	break; tr=tr.parentNode; }
			if(tr==null)	return;
		}

		if(state == null)	state = !(tr.selected==true?true:false);

		if(event==null || (!event.ctrlKey && !event.shiftKey) || !isMulCheckable )		/*只允许单一选择的情况*/
		{
			var thistr = tr.parentNode.firstChild;
			var clearCounter = 0;
			if(isInner==true || !isMulCheckable)
			{
				while(thistr != null)
				{
					if(thistr.selected==true)	{ clearCounter++; _SetRowSelectedState(thistr, false, isShowCheckBox, isExistListSelectedStyle, isInner); }
					thistr = thistr.nextSibling;
				}
				if(state==true || clearCounter>1)
				{
					_SetRowSelectedState(tr, true, isShowCheckBox, isExistListSelectedStyle, isInner);
				}
			}
			else if(tr.selected != state)
			{
				_SetRowSelectedState(tr, state, isShowCheckBox, isExistListSelectedStyle, isInner);
			}
			curRow = bkCurRow = tr;
		}
		else if(event.ctrlKey && !event.shiftKey)	/*如果按住Ctrl键时单击*/
		{
			_SetRowSelectedState(tr, state, isShowCheckBox, isExistListSelectedStyle, isInner);
			curRow = bkCurRow = tr;
		}
		else if(event.shiftKey)						/*如果按住Shift键时单击*/
		{
			var table = this.GetTableNode();
			if(!event.ctrlKey)
			{
				var thistr = tr.parentNode.firstChild;
				while(thistr != null)
				{
					_SetRowSelectedState(thistr, false, isShowCheckBox, isExistListSelectedStyle, isInner);
					thistr = thistr.nextSibling;
				}
			}

			if(curRow==null)	curRow = tr.parentNode.firstChild;
			var maxRowIndex = curRow.rowIndex>tr.rowIndex? curRow.rowIndex:tr.rowIndex;
			var minRowIndex = curRow.rowIndex>tr.rowIndex? tr.rowIndex:curRow.rowIndex;
			for(var index=minRowIndex; index<=maxRowIndex; index++)
			{
				var thistr = table.rows(index);
				_SetRowSelectedState(thistr, true, isShowCheckBox, isExistListSelectedStyle, isInner);
			}
			bkCurRow = tr;
		}
		_RemSelectRows(clientID);
	};

	/*清除所有选中的行*/
	this.UnSelectAllRows = function()
	{
		var table = this.GetTableNode();
		var isShowCheckBox = this.GetTableNode().isShowCheckBox.toLowerCase()=='true';
		var isExistListSelectedStyle = this.FindStyle('tbody.listselected')!=null;

		var tbody = this.GetNode('tbody');
		if(tbody==null)		return;
		var tr = tbody.firstChild;
		while(tr != null)
		{
			_SetRowSelectedState(tr, false, isShowCheckBox, isExistListSelectedStyle);
			tr = tr.nextSibling;
		}
		_RemSelectRows(clientID);
	};

	/*选择所有的行*/
	this.SelectAllRows = function()
	{
		var table = this.GetTableNode();
		var isMulCheckable = table.isMulCheckable.toLowerCase()=='true';
		if(!isMulCheckable)	return;

		var isShowCheckBox = table.isShowCheckBox.toLowerCase()=='true';
		var isExistListSelectedStyle = this.FindStyle('tbody.listselected')!=null;

		var tbody = this.GetNode('tbody');
		if(tbody==null)		return;
		var tr = tbody.firstChild;
		while(tr != null)
		{
			_SetRowSelectedState(tr, true, isShowCheckBox, isExistListSelectedStyle);
			tr = tr.nextSibling;
		}
		_RemSelectRows(clientID);
	};

	/*得到所选择的行*/
	this.GetSelectedRows = function()
	{
		var selectedRows = document.all(clientID + '_selectedrows').value;
		if(selectedRows == '')	return new Array();
		else					return selectedRows.split('[selectedrows]');
	};

	/*得到指定行标记的行，返回TR节点*/
	this.GetRowByFlag = function(flag)
	{
		var tbody = this.GetNode('tbody');
		if(tbody==null)		return;
		var tr = tbody.firstChild;
		while(tr!=null)
		{
			if(tr.flag==flag)	return tr;
			tr = tr.nextSibling;
		}
		return null;
	};

	/*得到过滤条件的节点*/
	this.GetFilterNode = function(name)
	{
		var node = document.all(clientID + '_' + name);
		if(node==null)	node = document.all(name);
		return node;
	};

	/*得到过滤条件节点的值*/
	this.GetFilterValue = function(name)
	{
		var node = this.GetFilterNode(name);
		if(node==null)	return null;
		return node.value;
	};

	/*设置过滤条件节点的值*/
	this.SetFilterValue = function(name, value)
	{
		var node = this.GetFilterNode(name);
		if(node==null)	return;
		if(node.value != value)
		{	node.value = value;
			node.fireEvent('onchange');
		}
	};

	/*增加一行*/
	this.Insert = function(rowFlag, arrValues, isInner)
	{
		if(!this.IsEditable)	return;
		var table = this.GetTableNode();
		var colMsg = window.eval(clientID + '_ColStyle');

		var tbody = this.GetNode('tbody');
		if(tbody==null)		return;
		var newRow = document.createElement('TR');
		tbody.appendChild(newRow);

		// 赋予一个临时的全局标识符
		if(rowFlag==null)	rowFlag = 'ROWFLAG_' + ((new Date).valueOf().toString() +  Math.random()).replace('.', '');
		newRow.flag = rowFlag;

		if(this.IsShowCheckBox)	// 显示复选/单选框
		{
			var newCell = newRow.insertCell();
			var myHtml = '<INPUT\tonclick=myReportCell.SelectRow(this,this.checked,false)\tstyle=width:16px;height:16px\t';
			if(this.IsMulCheckable)	myHtml += 'type=checkbox\t';
			else					myHtml += 'type=radio\tname=' + clientID + '_CHECKEDFLAG';
			myHtml += '>';
			newCell.innerHTML = myHtml;
			newCell.align = 'center';
			if(isInner!=true)	_RemSelectRows(clientID);
		}
		for(var index=0; index<colMsg.length; index++)
		{
			var newCell = newRow.insertCell();
			var className = colMsg[index][4];
			newCell.className = className;

			// 寻找指定列的原始值
			var value = null;
			if(arrValues!=null)
			{
				var colName = colMsg[index][3];
				for(var k=0; k<arrValues.length; k++)
					if(arrValues[k][0]==colName) { value = arrValues[k][1]; break; }
			}
			newCell.innerHTML = _WriteEditStateControl(clientID, value, index+1, rowFlag, 'insert');
		}

		if(isInner!=true)
		{
			_ResetRowClassName(clientID);
			_SchemaEditData(clientID);
		}
	};

	/*删除选中行*/
	this.Delete = function(row, isInner)
	{
		if(!this.IsEditable)	return;
		var selectedRows = null;
		if(row==null)	selectedRows = this.GetSelectedRows();
		else if(typeof(row)=='string')	selectedRows = new Array(row);
		else return;

		var tbody = this.GetNode('tbody');
		var valueArr = window.eval(clientID + '_EditMessage');

		if(selectedRows.length==0)	return;
		for(var index=0; index<selectedRows.length; index++)
		{
			var thisRow = this.GetRowByFlag(selectedRows[index]);
			var rowFlag = thisRow.flag;
			tbody.removeChild(thisRow);

			// 寻找并删除该行的后台数据
			var focusRow = null;
			var k = 0;
			for(;k<valueArr.length; k++)
			{
				var rowMsg = valueArr[k];
				if(rowMsg[0][0]==rowFlag)	{ focusRow = rowMsg; break; }
			}
			var rowState = focusRow[0][1];
			if(rowState=='insert')
			{
				for(;k<valueArr.length-1; k++)	valueArr[k] = valueArr[k+1];
				valueArr.length = valueArr.length-1;
			}
			else
			{
				focusRow[0][1] = 'delete';
			}
		}
		if(isInner!=true)
		{
			_RemSelectRows(clientID);
			_ResetRowClassName(clientID);

			// 实时序列化后台数据
			_SchemaEditData(clientID);
		}
	};

	/*设置某单元格的值*/
	this.SetValue = function(rowFlag, colName, newValue, isInner)
	{
		if(!this.IsEditable)	return;
		// 寻找相应的单元格
		if(isInner!=true)
		{
			var thisCtrl = document.getElementsByName(clientID + '$' + colName + '$' + rowFlag);
			if(thisCtrl.length==0)	return;

			var node = thisCtrl[0];
			var tagName = node.tagName;
			var type    = node.type;
			switch(tagName)
			{
				case 'INPUT':
					switch(type.toUpperCase())
					{
						case 'RADIO':		// 给单选框赋值
							for(var index=0; index<thisCtrl.length; index++)
							{
								var item = thisCtrl[index];
								var itemValue = item.value.toLowerCase();
								item.checked = newValue==null?false:itemValue==newValue.toLowerCase();
							}
							break;
						case 'CHECKBOX':	// 给复选框赋值
							var arrValues = newValue==null?(new Array()):newValue.toLowerCase().split(',');
							for(var index=0; index<thisCtrl.length; index++)
							{
								var item = thisCtrl[index];
								var itemValue = item.value.toLowerCase();
								for(var k=0; k<arrValues.length; k++)	if(arrValues[k]==itemValue)	break;
								item.checked = k!=arrValues.length;
							}
							break;
						default: node.value = newValue;
					} break;
				case 'SELECT':
					if(node.size>1&&node.multiple)	__SetListBoxValue(node, newValue);
					else	node.value = newValue;
					break;
				default: node.value = newValue;
			}
		}

		// 更新后台数据
		var valueArr = window.eval(clientID + '_EditMessage');
		// 寻找该行的节点（若不存在则新建）
		var focusRow = null;
		for(var index=0; index<valueArr.length; index++)
		{
			var rowMsg = valueArr[index];
			if(rowMsg[0][0]==rowFlag)	{ focusRow = rowMsg; break; }
		}
		if(focusRow==null) focusRow = valueArr[valueArr.length] = new Array( new Array(rowFlag,'insert') );

		// 寻找该列的节点（若不存在则新建）
		var focusNode = null;
		for(var index=1; index<focusRow.length; index++)
		{
			if(focusRow[index][0]==colName)	{ focusNode = focusRow[index]; break; }
		}
		if(focusNode==null)	focusNode = focusRow[focusRow.length] = new Array(colName, null);

		// 比较原始值，赋予新值并设置行的状态
		var oldValue  = focusNode[1];
		var focusFlag = focusRow[0];
		if(focusFlag[1]=='unchange' || focusFlag[1]=='update')
		{
			if(oldValue==newValue)	focusNode.length = 2;
			else					focusNode[2] = newValue;
			var changedCount = 0;
			for(var index=1; index<focusRow.length; index++)	if(focusRow[index].length==3)	changedCount++;
			focusFlag[1] = changedCount==0?'unchange':'update';
		}
		else if(focusFlag[1]=='insert')
		{
			focusNode[1] = newValue;
		}

		// 实时序列化后台数据
		if(isInner!=false)	_SchemaEditData(clientID);
	};

	/*读取某单元格的值*/
	this.GetValue = function(rowFlag, colName)
	{
		return null;
	};

	/*读取某单元格原先的值*/
	this.GetOldValue = function(rowFlag, colName)
	{
		return null;
	};

	/*保存编辑过的数据*/
	this.Save = function(message)
	{
		if(!this.IsEditable)	return;
		var valueNode = document.all(clientID + '_EditMessage');
		if(valueNode.value=='')		return;

		if(message==true)	message = '确认要保存吗？';
		this.SendMessage('Save', null, message);
	};

	/*一些临时参数*/
	this.EnterKeyAction = 40;	// 记录在编辑状态下，回车键发生的动作。
}

/*设置某一行的选中状态*/
function _SetRowSelectedState(thisRow, state, isShowCheckBox, isExistListSelectedStyle, isSetFocus)
{
	if(thisRow.selected==state)	return;
	thisRow.selected = state;
	if(isShowCheckBox)	{ var checkNode = thisRow.cells[0].firstChild; checkNode.checked = state; if(isSetFocus) checkNode.focus(); }
	if(state==false)	/*取消选中状态*/
	{
		if(thisRow.className == 'listselected')
		{
			if(thisRow.bkClassName!=null)	thisRow.className = thisRow.bkClassName;
		}
	}
	else				/*选中状态*/
	{
		if(isExistListSelectedStyle==null)	isExistListSelectedStyle = this.FindStyle('tbody.listselected')!=null;
		if(isExistListSelectedStyle)
		{
			if(thisRow.className!='listhover' && thisRow.className!='listselected') thisRow.bkClassName = thisRow.className;
			thisRow.className = 'listselected';
		}
	}
}

/*构建选择的行的列表*/
function _RemSelectRows(clientID)
{
	var selectedRows = '';
	var table = window.eval(clientID + '.GetTableNode()');
	var trBeginIndex = 0;
	var tbody = table.firstChild;
	while(tbody!=null && tbody.tagName != 'TBODY')	tbody = tbody.nextSibling;
	if(tbody==null)		return;
	var thistr = tbody.firstChild;
	var isSelectedAll = true;
	var isUnSelectedAll = true;

	if(thistr!=null) 
	{
		var beginIndex = thistr.rowIndex;
		while(thistr != null)
		{
			if(thistr.selected == true)
			{
				selectedRows += thistr.flag==null?thistr.rowIndex-beginIndex : thistr.flag;
				selectedRows += '[selectedrows]';
				isUnSelectedAll = false;
			}
			else	isSelectedAll = false;
			thistr = thistr.nextSibling;
		}
		if(selectedRows != '')		selectedRows = selectedRows.substring(0, selectedRows.length-14);
		if(document.all(clientID + '_selectedrows').value != selectedRows)
		{
			document.all(clientID + '_selectedrows').value = selectedRows;
			if(table.cell_onselect != null)	window.eval(table.cell_onselect.split('(')[0] + '(\'' + tr.flag + '\')');
		}
	}
	else
	{
		isSelectedAll   = false;
		isUnSelectedAll = true;
	}
	var selectedAllFlag = document.all(clientID + '_SelectedAllFlag');
	var isMulCheckable = table.isMulCheckable.toLowerCase()=='true';
	if(selectedAllFlag!=null)	selectedAllFlag.checked = isMulCheckable?isSelectedAll:isUnSelectedAll;
}


/*寻找所在的汇总屏*/
function _FindParentReportCell(node)
{
	while(node != null)
	{
		if(node.tagName=='TABLE' && node.id.toLowerCase().indexOf('reportcell')==0)	break;
		node = node.parentNode;
	}
	return node;
}

/*对导航按钮的支持*/
function _OnNavigation()
{
	var src = event.srcElement;
	if(src.tagName != 'A')		return;
	var span = src;
	while(span.tagName.toUpperCase() != 'SPAN')		span = span.parentNode;
	var id = span.reportcell;

	if(src.innerHTML == '＜' || src.innerHTML == '＞')
	{
		var begin = parseInt(src.parentNode.firstChild.nextSibling.innerHTML.replace(RegExp('&nbsp;', 'img'), ''));
		var end   = parseInt(src.parentNode.lastChild.previousSibling.innerHTML.replace(RegExp('&nbsp;', 'img'), ''));
		var length = end - begin + 1;
		var pagecount = window.eval(id + '.GetPageCount()', 'javascript');
		var curpage = window.eval(id + '.GetCurPageIndex()', 'javascript');

		if(src.innerHTML == '＜')
		{
			begin -= length;
			if(begin < 1)	begin = 1;
			end = begin + length - 1;
		}
		else if(src.innerHTML == '＞')
		{
			end += length;
			if(end > pagecount)		end = pagecount;
			begin = end - length + 1;
		}
		var thisNode = src.parentNode.firstChild.nextSibling;
		for(index=begin; index<=end; index++)
		{
			thisNode.innerHTML = '&nbsp;' + index + '&nbsp;';
			thisNode.className = curpage==index?'CurPage':'';
			thisNode = thisNode.nextSibling;
		}
	}
	else
	{
		var text = src.innerHTML.replace(RegExp('&nbsp;', 'img'), '');
		src.href = 'javascript:' + id + '.GotoPage(' + text + ')';
	}
}
/*向客户端过滤条件赋值*/
function _SetFilterValue(clientID)
{
	var arrValues = window.eval(clientID + '_FilterValue');
	var length = arrValues.length;
	for(var index=0; index<length; index++)
	{
		var value = arrValues[index][1];
		var name  = arrValues[index][0];
		var myObj = document.all(clientID + '_' + name);
		if(myObj == null)	myObj = document.all(name);
		if(myObj == null)	/*若不存在，则创建隐藏控件*/
		{
			myObj = document.createElement('input');
			myObj.type = 'hidden';
			myObj.name = clientID + '_' + name;
			myObj.id   = myObj.name;
			document.forms[0].insertBefore(myObj, document.forms[0].firstChild);
		}

		if(myObj.tagName != null)
		{
			if(myObj.name==null||myObj.name=='')	myObj.name = myObj.id;
			myObj.reportCell = clientID;
			myObj.attachEvent('onchange',_FilterChange);
			
			switch(myObj.tagName.toUpperCase())
			{
				case 'INPUT':
					switch(myObj.type.toUpperCase())
					{
						case 'CHECKBOX': 
							if(myObj.value==value)	myObj.checked = true;
							else					myObj.checked = false;
							break;
						case 'HIDDEN': case 'PASSWORD': case 'TEXT':
							myObj.value = value;
							break;
						case 'FILE': case 'BUTTON': case 'RESET': case 'SUBMIT': break;
						default: myObj.value = value; break;
					}
					break;
				case 'SELECT':		myObj.value = value; break;
				case 'TEXTAREA':	myObj.value = value; break;
				default:		/*其它*/
					try
					{
						if(myObj.value!=null)	myObj.value = value;
						else					myObj.innerHTML = value.replace(RegExp('&','img'), '&amp;').replace(RegExp('<', 'img'), '&lt;').replace(RegExp('>', 'img'), '&gt;')
													.replace(RegExp(' ', 'img'), '&nbsp;').replace(RegExp('""', 'img'), '&quot;').replace(RegExp('\n','img'), '<br>');
					}
					catch(e) {}
			}

			/*赋予该控件的onkeydown事件，按下回按的时候，自动执行查询*/
			myObj.attachEvent('onkeydown',function() { if(event.keyCode==13) window.eval(clientID+'.Refresh()')} );
		}
		else	/*如果是组合*/
		{
			var counter = myObj.length;
			/*设置过滤条件改变的事件*/
			for(var k=0; k<counter; k++) { myObj[k].reportCell=clientID; myObj[k].attachEvent('onchange',FilterChange); }
			switch(myObj[0].tagName.toUpperCase())
			{
				case 'INPUT':
					switch(myObj[0].type.toUpperCase())
					{
						case 'CHECKBOX':	/*复选框组合*/
							var arrValues = value.split(',');
							for(var k=0; k<counter; k++)
							{
								for(var j=0; j<arrValues.length; j++)
								{
									if(myObj[k].value == arrValues[j])
									{
										myObj[k].checked = true;
										break;
									}
								}
								if(j==arrValues.length) myObj[k].checked = false;
							}
							break;
						case 'RADIO':		/*单选框组合*/
							for(var k=0; k<counter; k++)
							{
								if(myObj[k].value == value)		myObj[k].checked = true;
								else							myObj[k].checked = false;
							}
							break;
					}
					break;
				default: break;
			}
		}
	}
}

/*过滤条件改变*/
function _FilterChange()
{
	var node = event.srcElement;
	var reportCell = node.reportCell;
	var table = window.eval(reportCell).GetTableNode();
	if(table.cell_onfilterchange != null)	window.eval(table.cell_onfilterchange.split('(')[0] + '(node)');
}

/*响应汇总屏的事件*/
function _ReportCellEvent()
{
	var src = event.srcElement;
	var srcTagName = src.tagName;
	var td, tr, tbody, thead, tfoot, table;

	var thisNode = src;
	while(true)
	{
		try
		{
			window.eval(thisNode.tagName.toLowerCase() + ' = thisNode ');
			if(thisNode.tagName == 'TABLE')	break;
			thisNode = thisNode.parentNode;
		} catch(e) { return; }
	}
	try{

	var postion = table.id.indexOf('_');
	var cell = window.eval(table.operate);

	switch(event.type.toLowerCase())
	{
		case 'mouseover': if(tr!=null && tbody!=null)
						  {	if(tr.className!='listselected' && cell.FindStyle('tbody.listhover')!=null && tr.className!='listhover') { tr.bkClassName = tr.className; tr.className = 'listhover'; } 
							try{if(tr!=null && tbody!=null) if(table.cell_onmouseover != null)  window.eval(table.cell_onmouseover.split('(')[0] + '(\'' + tr.flag + '\')');}catch(e){window.status='脚本错误：'+e.message;}
						  } break;
		case 'mouseout':  if(tr!=null && tbody!=null)
						  { if(tr.className!='listselected' && tr.bkClassName!=null && tr.className!=tr.bkClassName) { tr.className = tr.bkClassName; } 
							try{if(tr!=null && tbody!=null) if(table.cell_onmouseout != null)  window.eval(table.cell_onmouseout.split('(')[0] + '(\'' + tr.flag + '\')'); }catch(e){window.status='脚本错误：'+e.message;}
						  } break;
		case 'click':	  if(tr!=null && tbody!=null)
						  { var arrTag = new Array('INPUT', 'SELECT', 'TEXTAREA', 'LABEL', 'A', 'BUTTON');
							for(var index=0; index<arrTag.length; index++)	if(arrTag[index]==srcTagName) break;
							if(index!=arrTag.length)	break;
							if(srcTagName=='TD' && src.isCheckBox=='true')	break;
							cell.SelectRow(tr, null, true); 
							try{if(table.cell_onclick != null)	window.eval(table.cell_onclick.split('(')[0] + '(\'' + tr.flag + '\')');}catch(e){window.status='脚本错误：'+e.message; }
							if(tr.selected==true && cell.GetSelectedRows().length==1 && table.cell_onbrowse != null)	window.eval(table.cell_onbrowse.split('(')[0] + '(\'' + tr.flag + '\')');
						  } break;
		case 'keyup':	  if(tr!=null && tbody!=null)
						  { if(event.keyCode==16) cell._SetCurRow();
						  } break;
		case 'dblclick':  if(tr!=null && tbody!=null)
						  {	var arrTag = new Array('INPUT', 'SELECT', 'TEXTAREA', 'A', 'BUTTON');
							for(var index=0; index<arrTag.length; index++)	if(arrTag[index]==srcTagName) break;
							if(index!=arrTag.length)	break;
							if(srcTagName=='TD' && src.isCheckBox=='true')	break;
							cell.SelectRow(tr, true, true); 
						    try{if(table.cell_ondblclick != null)  window.eval(table.cell_ondblclick.split('(')[0] + '(\'' + tr.flag + '\')');}catch(e){window.status='脚本错误：'+e.message;}
						  }	break;
	}
	}catch(e){}
}

/*选中默认的行*/
function _SelectDefaultRows(clientID)
{
	var table = window.eval(clientID + '.GetTableNode()');
	var isShowCheckBox = table.isShowCheckBox.toLowerCase()=='true';
	var isMulCheckable = table.isMulCheckable.toLowerCase()=='true';
	var selectedRows = document.all(clientID + '_SelectedRows').value;
	var isExistListSelectedStyle = window.eval(clientID + "".FindStyle('tbody.listselected')!=null"");

	var arrSelectRows = selectedRows.split('[selectedrows]');
	if(!isMulCheckable && arrSelectRows.length>1)	arrSelectRows.length = 1;

	var trBeginIndex = 0;
	var tbody = table.firstChild;
	while(tbody!=null && tbody.tagName != 'TBODY')	tbody = tbody.nextSibling;
	if(tbody==null || tbody.children.length==0)		return;
	trBeginIndex = tbody.firstChild.rowIndex;

	for(var k=0; k<arrSelectRows.length; k++)
	{
		var flag = arrSelectRows[k];
		var thistr = tbody.firstChild;
		while(thistr != null)
		{
			if( (thistr.flag==null&&thistr.rowIndex-trBeginIndex==parseInt(flag)) || thistr.flag == flag )
			{
				_SetRowSelectedState(thistr, true, isShowCheckBox, isExistListSelectedStyle);
			}
			thistr = thistr.nextSibling;
		}
	}
	_RemSelectRows(clientID);
}

/*增添汇总屏的事件*/
function _AttachReportCellEvent(clientID)
{
	var table = window.eval(clientID + '.GetTableNode()');
	
	__AttachReportCellEvent(table, 'onmouseover', _ReportCellEvent);
	__AttachReportCellEvent(table, 'onmouseout',  _ReportCellEvent);
	__AttachReportCellEvent(table, 'onclick',	  _ReportCellEvent);
	__AttachReportCellEvent(table, 'ondblclick',  _ReportCellEvent);
	__AttachReportCellEvent(table, 'onkeyup',	  _ReportCellEvent);
	__AttachReportCellEvent(table, 'onselectstart', function()
		{
			var srcTagName = event.srcElement.tagName;
			var arrTag = new Array('INPUT', 'TEXTAREA');
			for(var index=0; index<arrTag.length; index++)	if(arrTag[index]==srcTagName) break;
			if(index!=arrTag.length)	return;
			if(!window.eval(clientID).IsSelectable || event.shiftKey || event.ctrlKey)	event.returnValue = false; 
		} 
	);
}

function __AttachReportCellEvent(node, eventName, newEvent)
{
	var oldEvent = window.eval('node.' + eventName);
	window.eval('node.' + eventName + '= newEvent');
	if(oldEvent!=null)	node.attachEvent(eventName, oldEvent);
}

/*在可编辑状态中写入可编辑控件*/
function _WriteEditStateControl(clientID, content, colIndex, rowFlag, rowState)
{
	var colStyle = window.eval(clientID + '_ColStyle');
	var msg = colStyle[colIndex-1];
	var editControl = msg[0];
	var editControlType = msg[1];
	var editControlData = msg[2];
	var colName	  = msg[3];
	// var className = msg[4];
	var width     = msg[5];
	var tempMsg   = msg[6];
	var id	 = (clientID + '$' + colName + '$' + rowFlag).replace(/""/img, '&quot;');
	var name = id;
	var result = '';
	var isEditControl = true;
	var isEmpty = false;
	if(content==null) { content=''; isEmpty=true; }

	switch(editControl)
	{
		case 'textbox':		/*文本框*/
			result += '<input\ttype=inputbox\tid=""' + id + '""\t' + editControlType;
			result += '\tvalue=""' + content.replace(/""/img, '&quot;') + '""';
			result += '\tstyle=""width:' + width + '""\t';
			result += '\tonchange=_EditControlValueChanged()\tonkeydown=_EditControlChangeFocus()\toncontextmenu=_EditControlShowMenu()>';
			break;
		case 'textarea':	/*多行文本框*/
			result += '<textarea\tid=""' + id + '""\t' + editControlType;
			result += '\tstyle=""width:' + width + ';height:100%;""\t';
			result += 'onchange=_EditControlValueChanged()\tonkeydown=_EditControlChangeFocus()\toncontextmenu=_EditControlShowMenu()>';
			result += content;
			result += '</textarea>';
			break;
		case 'checkbox':	/*复选框*/
			content = content.toLowerCase();
			if(tempMsg==null)
			{
				tempMsg = new Array(new Array('True','False'), '是/否');
				if(editControlData!='')
				{
					var tempArr = editControlData.split('|');
					var trueMsg = tempArr[0].split('/');
					tempMsg[0][0] = trueMsg[0];
					if(tempArr.length<2)	tempArr[1] = 'false/否';
					var falseMsg = tempArr[1].split('/');
					tempMsg[0][1] = falseMsg[0];
					tempMsg[1] = (trueMsg[trueMsg.length-1] + '/' + falseMsg[falseMsg.length-1]).replace(/""/img, '\\\""');
				}
				msg[6] = tempMsg;
			}
			var trueValue=tempMsg[0][0], falseValue=tempMsg[0][1], title=tempMsg[1];
			result += '<input\ttype=checkbox\tstyle=width:16px;height:16px\t' + editControlType;
			result += '\tvalue=""' + trueValue + '""\tid=""' + id + '""';
			if(content==trueValue.toLowerCase())		result += '\tchecked\t';
			else if(content!=falseValue.toLowerCase())	result += '\tdisabled\t';
			result += 'title=' + tempMsg[1] + '\tonclick=_EditControlValueChanged()\tonkeydown=_EditControlChangeFocus()\toncontextmenu=_EditControlShowMenu()>';
			break;
		case 'checklist':	/*复选框列表*/
			content = content.toLowerCase();
			if(tempMsg==null)
			{
				tempMsg = new Array();
				if(editControlMessage!='')
				{
					var tempArr = editControlData.split('|');
					for(var index=0; index<tempArr.length; index++)
					{
						var arrItem = tempArr[index].split('/');
						if(arrItem.length==1)	arrItem[1] = arrItem[0];
						tempMsg[tempMsg.length] = arrItem;
					}
				}
				msg[6] = tempMsg;
			}
			for(var index=0; index<tempMsg.length; index++)
			{
				var value = tempMsg[index][0], text = tempMsg[index][1];
				var isChecked = false, arrValues = content.split(',');;
				for(var k=0; k<arrValues.length; k++)	if(arrValues[k]==value.toLowerCase())	isChecked = true;
				result += '<input\ttype=checkbox\tstyle=width:16px;height:16px\tvalue=""' + value + '""';
				if(isChecked) result += '\tchecked\t';
				result += '\tonclick=_EditControlValueChanged()\tonkeydown=_EditControlChangeFocus()\toncontextmenu=_EditControlShowMenu()';
				result += '\tname=""' + name + '""';
				result += '\tid=""' + id + index + '""\t' + editControlType;
				result += '><label\tfor=""' + id + index + '"">' + text + '&nbsp;</label>';
			}
			break;
		case 'combobox':	case 'listbox':		/*下拉列表框与多选列表框*/
			content = content.toLowerCase();
			if(tempMsg==null)
			{
				tempMsg = '';
				var arrMsg = editControlData.split('|');
				for(var index=0; index<arrMsg.length; index++)
				{
					if(arrMsg[index]=='')	continue;
					var tempArr = arrMsg[index].split('/');
					var value = tempArr[0].toLowerCase();
					var text  = tempArr[tempArr.length-1];
					tempMsg +=  '<option\tvalue=""' + value.replace(/""/img, '&quot;') + '"">';
					tempMsg += text;
					tempMsg += '</option>';
				}
				msg[6] = tempMsg;
			}
			if(editControl=='combobox')	editControlType = editControlType.replace(/multiple|size\s*=\s*\S*/img, '');
			result += '<select\tid=""' + id + '""\t' + editControlType;
			if(editControl=='listbox') result += '\tsize=3\tmultiple\tstyle=""height:100%""';
			result += '\tstyle=""width:' + width + '""';
			result += '\tonchange=_EditControlValueChanged()\tonkeydown=_EditControlChangeFocus()\toncontextmenu=_EditControlShowMenu()>';
			result += tempMsg;
			result += '</select>';
			if(editControl=='combobox')	result += '<script\tlanguage=javascript>document.all(""' + id + '"").value=""' + content + '"";</' + 'script>';
			else	result += '<script\tlanguage=javascript>__SetListBoxValue(""' + id + '"",""' + content.replace(/""/img, '\""')  + '"");</' + 'script>';
			break;
		case 'radio':		/*单选框*/
			content = content.toLowerCase();
			if(tempMsg==null)
			{
				tempMsg = new Array();
				var arrMsg = editControlData.split('|');
				for(var index=0; index<arrMsg.length; index++)
				{
					if(arrMsg[index]=='')	continue;
					var tempArr = arrMsg[index].split('/');
					tempMsg[index] = new Array();
					tempMsg[index][0] = tempArr[0];
					tempMsg[index][1] = tempArr[tempArr.length-1];
				}
				msg[6] = tempMsg;
			}
			for(var index=0; index<tempMsg.length; index++)
			{
				var value = tempMsg[index][0], text = tempMsg[index][1];
				result += '<input\tstyle=width:16px;height:16px\ttype=radio\tid=""' + id + index + '"" ';
				result += '\tvalue=""' + value + '""\t' + editControlType;
				result += '\tonclick=_EditControlValueChanged()\tonkeydown=_EditControlChangeFocus()\toncontextmenu=_EditControlShowMenu()';
				if(content==value.toLowerCase())	result += '\tchecked\t';
				result += '\tname=""' + name + '"">';
				result += '<label\tfor=""' + id + index + '"">' + text + '&nbsp;</label>';
			}
			break;
		default:			/*其它未知的控件*/
			isEditControl = false;
			if(editControl=='' && content=='')	result += '&nbsp;';
			else								result += content;
			break;
	}

	// 记住各编辑控件的值
	if(isEditControl)
	{
		if(isEmpty)	content = null;
		var valueArr = window.eval(clientID + '_EditMessage');
		var length   = valueArr.length;
		if(rowState==null)	rowState = 'unchange';
		if(length==0 || valueArr[length-1][0][0]!=rowFlag)	valueArr[length] = new Array( new Array(rowFlag, rowState) );
		valueArr[valueArr.length-1][valueArr[valueArr.length-1].length] = new Array(colName, content);
	}
	return result;
}

function _W19L(clientID, content, colIndex, rowFlag, rowState)
{
	document.write(_WriteEditStateControl(clientID, content, colIndex, rowFlag, rowState));
}

/*设置多选列表控件的值*/
function __SetListBoxValue(id, value)
{
	var node = typeof(id)=='string'?document.all(id) : id;
	if(node==null)	return;
	var arrValues = value==null?(new Array()):value.toLowerCase().split(',');
	for(index=0; index<node.options.length; index++)
	{
		var item = node.options[index];
		var value = item.value.toLowerCase();
		for(var k=0; k<arrValues.length; k++)	if(arrValues[k]==value)	break;
		item.selected = k!=arrValues.length;
	}
}

/*编辑控件的值发生变化*/
function _EditControlValueChanged()
{
	var src = event.srcElement;
	var valueTag = src.name==null||src.name==''?src.id:src.name;
	var arrTemp = valueTag.split('$');
	var clientID = arrTemp[0];
	var colName  = arrTemp[1];
	var rowFlag  = valueTag.substring(clientID.length+colName.length+2);
	var table = window.eval(clientID).GetTableNode();
	var newValue = null;

	// 读取现在的值
	switch(src.tagName)
	{
		case 'INPUT':
			switch(src.type.toUpperCase())
			{
				case 'RADIO':
					var arrNodes = document.getElementsByName(valueTag);
					for(var index=0; index<arrNodes.length; index++)
					{
						if(arrNodes[index].checked) { newValue = arrNodes[index].value; break; }
					}
					if(newValue!=null)	newValue = newValue.toLowerCase();
					break;
				case 'CHECKBOX':
					var arrNodes = document.getElementsByName(valueTag);
					newValue = '';
					for(var index=0; index<arrNodes.length; index++)
					{
						if(arrNodes[index].checked)
						{
							if(newValue!='')	newValue += ',';
							newValue += arrNodes[index].value;
						}
					}
					newValue = newValue.toLowerCase();
					break;
				default: newValue = src.value;
			} break;
		case 'SELECT':
			if(src.size>1 && src.multiple)
			{
				newValue = '';
				for(var index=0; index<src.options.length; index++)
				{
					var item = src.options[index];
					if(item.selected) { if(newValue!='') newValue += ','; newValue += item.value; }
				}
				newValue = newValue.toLowerCase();
			}
			else
			{
				newValue = src.value;
				if(newValue!=null)	newValue = newValue.toLowerCase();
			}
			break;
		default: newValue = src.value; break;
	}

	// 同步后台数据
	window.eval(clientID).SetValue(rowFlag, colName, newValue, true);
}

/*编辑数据时切换焦点*/
function _EditControlChangeFocus()
{
	var src = event.srcElement, tagName = src.tagName;
	var keyCode = event.keyCode;
	var valueTag = src.name==null||src.name==''?src.id:src.name;
	var arrTemp = valueTag.split('$');
	var clientID = arrTemp[0], colName = arrTemp[1];
	var reportCell = window.eval(clientID);

	if(keyCode==37||keyCode==39)
	{
		var type = src.type.toLowerCase();
		if((tagName=='TEXTAREA'||(tagName=='INPUT'&&(type=='text'||type=='radio'||type=='checkbox')))) { if(!event.altKey) return; }
		else if(tagName!='SELECT' && !event.ctrlKey) return;
	}
	else if(keyCode==13)		 { if(tagName=='TEXTAREA'&&event.shiftKey) return; else { keyCode=reportCell.EnterKeyAction; if(event.altKey) keyCode=keyCode==37?39:keyCode==39?37:keyCode==38?40:keyCode==40?38:0; } }
	else if(tagName=='TEXTAREA') { if(keyCode!=38&&keyCode!=40 || !event.altKey) return; }
	else if(tagName=='INPUT')    { if(keyCode!=38 && keyCode!=40)	return; }
	else if(tagName=='SELECT')   { if(!((keyCode==38||keyCode==40) && !event.altKey && event.ctrlKey)) return; }

	reportCell.EnterKeyAction = keyCode;

	var row = src;
	while(row.tagName!='TR')	row = row.parentNode;

	if(keyCode==38 || keyCode==40)
	{
		if(event.keyCode==13)
		{
			if(keyCode==38&&row.previousSibling==null)	keyCode=40;
			else if(keyCode==40&&row.nextSibling==null)	keyCode=38;
			reportCell.EnterKeyAction = keyCode;
		}
		while(true)
		{
			row = keyCode==38?row.previousSibling : row.nextSibling;
			if(row==null)	break;
			var nodes = document.getElementsByName(clientID + '$' + colName + '$' + row.flag);
			if(nodes.length>0)
			{
				var node = nodes[0];
				for(var k=0; k<nodes.length; k++)	if(nodes[k].checked) { node = nodes[k]; break; }
				try { node.focus(); } catch(e) { continue; }
				if(node.tagName=='INPUT'&&node.type.toLowerCase()=='text' || node.tagName=='TEXTAREA')
					node.createTextRange().select();
				else document.selection.empty();
				break;
			}
		}
	}
	else if(keyCode==37 || keyCode==39)
	{
		var colStyle = window.eval(clientID + '_ColStyle');
		for(var index=0; index<colStyle.length; index++) if(colStyle[index][3]==colName) break;
		if(index!=colStyle.length)
		{
			while(true)
			{
				index = keyCode==37?index-1:index+1;
				if(index<0 || index>=colStyle.length)	break;
				if(colStyle[index][0]=='')	continue;
				var nodes = document.getElementsByName(clientID + '$' + colStyle[index][3] + '$' + row.flag);
				if(nodes.length>0)
				{
					var node = nodes[0];
					for(var k=0; k<nodes.length; k++)	if(nodes[k].checked) { node = nodes[k]; break; }
					try { node.focus(); } catch(e) { continue; }
					if(node.tagName=='INPUT'&&node.type.toLowerCase()=='text' || node.tagName=='TEXTAREA')
						node.createTextRange().select();
					else document.selection.empty();
					break;
				}
			}
		}
	}
	event.returnValue = false;
}

/*编辑控件的右键菜单*/
function _EditControlShowMenu()
{
	var menuArr = new Array(
		new Array('取消更改', ''),
		new Array('设为空值', ''),
		new Array('-'),
		new Array('剪切', 'document.execCommand(""Cut"")'),
		new Array('复制', 'document.execCommand(""Copy"")'),
		new Array('粘帖', 'document.execCommand(""Paste"")')
	);

	var menu = new ReportMenu(menuArr, 75);
	menu.Show();
}

/*重置行的样式序列，在删除或增加行后调用*/
function _ResetRowClassName(clientID)
{
	if(!window.eval(clientID).IsEditable)	return;
	var rowMsg = window.eval(clientID + '_RowStyle');
	var difRow = rowMsg[0];
	var table = window.eval(clientID).GetTableNode();
	var tbody = window.eval(clientID).GetNode('tbody');
	if(tbody==null)	return;

	var tr = tbody.firstChild;
	while(tr!=null)
	{
		var rowIndex = tr.rowIndex - (table.tHead==null?0:table.tHead.rows.length);
		var styleIndex = (rowIndex%difRow) + 1;
		var className = null;
		for(var index=1; index<rowMsg.length; index++)
		{
			if(rowMsg[index][0]==styleIndex) { className = rowMsg[index][1]; break; }
		}
		if(className==null)	className = 'TR' + styleIndex;
		if(tr.className=='listhover'||tr.className=='listselected')		tr.bkClassName = className;
		else															tr.className   = className;
		tr = tr.nextSibling;
	}
}

function __Encode(str)
{
	if(str==null)	return null;
	str = str.replace(/#/mg, '#x');
	str = str.replace(/</mg, '#l');
	str = str.replace(/>/mg, '#g');
	str = str.replace(/""/mg, '#q');
	str = str.replace(/&/mg, '#a');
	str = str.replace(/\r\n/mg, '#n');
	return str;
}

function __Decode(str)
{
	if(str==null)	return null;
	str = str.replace(/#l/mg, '<');
	str = str.replace(/#g/mg, '>');
	str = str.replace(/#q/mg, '""');
	str = str.replace(/#a/mg, '&');
	str = str.replace(/#n/mg, '\r\n');
	str = str.replace(/#x/mg, '#');
	return str;
}

/*序列化后台数据*/
function _SchemaEditData(clientID)
{
	var valueArr  = window.eval(clientID + '_EditMessage');
	var valueNode = document.all(clientID + '_EditMessage');
	var xmlData = new String('<?xml\tversion=""1.0""\tencoding=""utf-8""?>\n');

	xmlData += '<ReportCellData>\n';
	var isEmpty = true;
	for(var index=0; index<valueArr.length; index++)
	{
		var thisRow = valueArr[index];
		var name = thisRow[0][0], state = thisRow[0][1];
		if(state=='unchange')	continue;
		isEmpty = false;
		xmlData += '<Row\tflag=""' + __Encode(name) + '""\tstate=""' + state + '"">';

		for(var k=1; k<thisRow.length; k++)
		{
			var thisCell = thisRow[k];
			var name = thisCell[0], value = thisCell[1], newValue = thisCell[2];
			if(state=='update')
			{
				if(newValue==null)	continue;
				xmlData += '<Cell\tname=""' + __Encode(name) + '"">';
				xmlData += '<Value>' + __Encode(value) + '</Value>';
				if(newValue!=null)	xmlData += '<NewValue>' + __Encode(newValue) + '</NewValue>';
				xmlData += '</Cell>';
			}
			else if(state=='insert')
			{
				if(value==null)	continue;
				xmlData += '<Cell\tname=""' + __Encode(name) + '"">';
				xmlData += '<Value>' + __Encode(value) + '</Value>';
				xmlData += '</Cell>';
			}
			else if(state=='delete')
			{
				// Do Nothing
			}
		}

		xmlData += '</Row>\n';
	}
	xmlData += '</ReportCellData>';
	valueNode.value = isEmpty?'':__Encode(xmlData);
}

/*反序列化后台数据*/
function _UnSchemaEditData(clientID)
{
	var valueArr  = window.eval(clientID + '_EditMessage');
	var valueNode = document.all(clientID + '_EditMessage');

	var xmlData = new String(valueNode.value);
	if(xmlData=='')		return;

	// 开始分析数据
	xmlData = __Decode(xmlData);
	var arrRows = xmlData.match(/<Row.*?<\/Row>/mg);
	if(arrRows==null)	return;

	var valueArr = window.eval(clientID + '_EditMessage');
	var reportCell = window.eval(clientID);

	for(var index=0; index<arrRows.length; index++)
	{
		var row = arrRows[index];
		var rowRegex  = /<Row\s+flag=""(.*?)""\s+state=""(.*?)""/m;
		var rowMsg = rowRegex.exec(row);
		var flag = __Decode(rowMsg[1]), state = __Decode(rowMsg[2]);
		var length = valueArr.length;

		if(state=='delete')			// 该行已被删除
		{
			reportCell.Delete(flag, true);
		}
		else if(state=='insert')	// 该行为新行
		{
			var arrCells = row.match(/<Cell.*?<\/Cell>/mg);
			var arrValues = new Array();
			if(arrCells!=null)
			{
				for(var k=0; k<arrCells.length; k++)
				{
					var cell = arrCells[k];
					var cellRegex = /<Cell\s+name=""(.*?)"">\s*<Value>(.*?)<\/Value>\s*<\/Cell>/m;
					var cellMsg = cellRegex.exec(cell);
					var name = __Decode(cellMsg[1]), value = __Decode(cellMsg[2]);
					arrValues[arrValues.length] = new Array(name, value);
				}
			}
			window.eval('reportCell.Insert(""' + flag.replace('""', '\\""') + '"", arrValues, true)');
		}
		else if(state=='update')	// 该行已被更改
		{
			var arrCells = row.match(/<Cell.*?<\/Cell>/mg);
			if(arrCells==null)	continue;
			for(var k=0; k<arrCells.length; k++)
			{
				var cell = arrCells[k];
				var cellRegex = /<Cell\s+name=""(.*?)"">\s*<Value>(.*?)<\/Value>\s*<NewValue>(.*?)<\/NewValue>\s*<\/Cell>/m;
				var cellMsg = cellRegex.exec(cell);
				var name = __Decode(cellMsg[1]), value = __Decode(cellMsg[2]), newValue = __Decode(cellMsg[3]);
				reportCell.SetValue(flag, name, newValue, false);
			}
		}
	}
	_SchemaEditData(clientID);
}",
			"ReportCell.Js");

		#endregion

		#region 客户端对细节屏操作的一些脚本

		/// <summary>
		/// 对细节屏脚本的访问
		/// </summary>
		static public string ReportDetailOperateScript
		{
			get
			{
				return _ReportDetailOperateScript.Script;
			}
		}

		static private ScriptOperate _ReportDetailOperateScript = new ScriptOperate(
@"
/*方便客户端对细节屏操作*/
function ReportDetailOperate(clientID, arrValues)
{
	/*执行更新操作*/
	this.Update = function(message)
	{
		if(message == true)		message = '即将更新数据，是否继续？';
		var result = this.TestContent();
		if(result == false)	{ alert(this.ErrorMsg);  if(this.ErrorNode!=null)	try{this.ErrorNode.focus();}catch(e){}  return; }
		this.SendMessage('Update', null, message);
	};

	/*执行插入操作*/
	this.Insert = function(message)
	{
		if(message == true)		message = '即将插入数据，是否继续？';
		var result = this.TestContent();
		if(result == false)	{ alert(this.ErrorMsg); if(this.ErrorNode!=null) try{this.ErrorNode.focus();}catch(e){}  return; }
		this.SendMessage('Insert', null, message);
	};

	/*删除当前项*/
	this.Delete = function(message)
	{
		if(message == true)		message = '即将删除数据，是否继续？';
		var result = this.TestContent();
		if(result != false)	{ alert(this.ErrorMsg); if(this.ErrorNode!=null) try{this.ErrorNode.focus();}catch(e){}  return; }
		this.SendMessage('Delete', null, message);
	};

	/*根据情况判断是执行插入还是更新*/
	this.Save = function(message)
	{
		if(message == true)		message = '即将保存数据，是否继续？';
		if(this.IsUpdateState())		this.Update(message);
		else							this.Insert(message);
	};

	/*刷新数据*/
	this.Reset = function(message)
	{
		if(message == true)		message = '即将重置所有数据，是否继续？';
		this.SendMessage('Reset', null, message);
	};

	/*是否为更新状态*/
	this.IsUpdateState = function()
	{
		var keys = 0;
		for(var index=0; index<arrValues.length; index++)
		{
			if(arrValues[index][2].toLowerCase() == 'true')
			{
				if(arrValues[index][1] == '')	return false;
				keys ++;
			}
		}
		if(keys==0)		return false;	/*若没有主键，则皆为插入状态*/
		return true;
	};

	/*验证内容是否符合规范，如果符合，返回true，否则返回错误消息！*/
	this.TestContent = function()
	{
		SendHtcMessage('TestContent');
		return _testContent(clientID, arrValues);
	};

	/*出现错误的HTML元素*/
	this.ErrorNode = null;

	/*错误消息*/
	this.ErrorMsg = null;

	/*向服务器端发送消息*/
	this.SendMessage = function(command, argument, message)
	{
		if(message==true)	message = '即将执行操作，是否继续？';
		if(message!=false && message!=null && window.confirm(message)==false)	return;
		document.all(clientID + '_Operate').value = command;
		document.all(clientID + '_Argument').value = argument==null?'':argument;
		if(SendHtcMessage(command)!=false)	document.forms[0].submit();
	};

	/*清空所有的项*/
	this.Clear = function()
	{try{
		var length = arrValues.length;
		for(var index=0; index<length; index++)
		{
			if(arrValues[index][2].toLowerCase() == 'true')		continue;
			var myObj = document.all(clientID + '_' + arrValues[index][0]);
			if(myObj==null)		myObj = document.all(arrValues[index][0]);
			if(myObj==null)		continue;
			if(myObj.tagName != null)
			{
				switch(myObj.tagName.toUpperCase())
				{
					case 'INPUT':
					switch(myObj.type.toUpperCase())
					{
						case 'CHECKBOX':	case 'RADIO':	myObj.checked = false; break;
						default:	myObj.value = '';
					}
					default: myObj.value = '';
				}
			}
			else for(var k=0; k<myObj.length; k++)
			{
				var itemObj = myObj[k];
				switch(itemObj.tagName.toUpperCase())
				{
					case 'INPUT':
					switch(itemObj.type.toUpperCase())
					{
						case 'CHECKBOX':	case 'RADIO':	itemObj.checked = false; break;
						default:	itemObj.value = '';
					}
					default: itemObj.value = '';
				}
			}
		}
		}catch(e){}
	};
}

/*在执行操作之前通知所有的HTC*/
function SendHtcMessage(msg, parentNode)
{
	if(parentNode==null)	parentNode = document.forms[0];
	for(var k=0; k<parentNode.children.length; k++)
	{
		var curNode = parentNode.children[k];
		if(curNode.scopeName == 'userdefinecomponent')
		{
			try { if(curNode.SendHtcMessage(msg)==false) return false; } catch(e){}
		}
		if(curNode.children!=null && curNode.children.length != 0)
		{
			if(SendHtcMessage(msg, curNode)==false)	return false;
		}
	}
	return true;
}

/*验证数据*/
function _testContent(clientID, testContentArr)
{
	for(var index=0; index<testContentArr.length; index++)
	{
		var myObj = document.all(clientID + '_' + testContentArr[index][0]);
		var Value = null;
		if(myObj==null)		myObj = document.all('htc_' + testContentArr[index][0]);
		if(myObj==null)		myObj = document.all(testContentArr[index][0]);
		if(myObj==null)		/*检查是否为自定义序列*/
		{
			var pattern = testContentArr[index][10];
			if(pattern=='')		continue;
			var myRegExp = new RegExp('\\{\\S+?\\}', 'mg');
			var result = pattern.match(myRegExp);
			if(result == null)		continue;
			for(var k=0; k<result.length; k++)
			{
				var tagId = result[k].substring(1, result[k].length-1);
				var itemValue = '';
				var itemObj = document.all(clientID + '_' + tagId);
				if(itemObj==null)	itemObj = document.all(tagId);
				if(itemObj!=null)	itemValue = GetHtmlValue(itemObj);
				if(itemValue == null)	itemValue = '';
				pattern = pattern.replace(result[k], itemValue);

				if(myObj==null)		myObj = itemObj;
			}
			Value = pattern;
		}
		else
		{
			Value = GetHtmlValue(myObj);
		}
		if(myObj.tagName == null)	myObj = myObj[0];

		var IsUpdateState = window.eval(clientID + '.IsUpdateState()');

		/*验证能否为空*/
		if(Value=='')
		{
			if(testContentArr[index][5].toLowerCase()=='false')
			{
				window.eval(clientID + '.ErrorNode = myObj');
				window.eval(clientID + '.ErrorMsg = testContentArr[index][8] + \'不能为空！\'');
				return false;
			}
			continue;
		}

		/*验证主键是否被改变*/
		if(IsUpdateState && (testContentArr[index][2].toLowerCase()=='true' || testContentArr[index][11].toLowerCase()=='false'))
		{
			if(testContentArr[index][1] != Value)
			{
				window.eval(clientID + '.ErrorNode = myObj');
				window.eval(clientID + '.ErrorMsg =  \'作为主键的字段“\' + testContentArr[index][8] + \'”不能再被改变！\'');
			}
		}

		/*验证是否符合规范*/
		if(testContentArr[index][6] != '')
		{
			var myRegExp = new RegExp(testContentArr[index][6], 'img');
			if(myRegExp.exec(Value)==null)
			{
				window.eval(clientID + '.ErrorNode = myObj');
				if(testContentArr[index][7] != '')		window.eval(clientID + '.ErrorMsg = testContentArr[index][7] ');
				else	window.eval(clientID + '.ErrorMsg = testContentArr[index][8] + \'不符合要求！\'');
				return false;
			}
		}
	}
	return true;
}

/*得到某元素或数组的值*/
function GetHtmlValue(myObj)
{
	var value = '';
	if(myObj == null)	return null;
	if(myObj.tagName!=null)
	{
		return myObj.value;
	}
	else
	{
		switch(myObj[0].tagName.toUpperCase())
		{
			case 'INPUT':
				switch(myObj[0].type.toUpperCase())
				{
					case 'RADIO':
						for(var k=0; k<myObj.length; k++)
							if(myObj[k].checked) { value = myObj[k].value; break; }
						break;
					case 'CHECKBOX':
						for(var k=0; k<myObj.length; k++)
							if(myObj[k].checked) value += ',' + myObj.value;
						if(value.length>0)	value = value.substring(1, value.length-1);
						break;
				}
		}
	}
	return value;
}

/*在客户端为细节屏各元素赋值*/
function ReportDetailSetValue(clientID, arrValues)
{
	var length = arrValues.length;
	for(var index=0; index<length; index++)
	{
		var value = arrValues[index][1];
		var isKey = arrValues[index][2].toLowerCase()=='true';
		var isUpdateable = arrValues[index][11].toLowerCase()=='true';
		var isSetReadOnly = isKey || !isUpdateable;

		/*得到要赋值的对象*/
		var myObj = document.all(clientID + '_' + arrValues[index][0]);
		if(myObj==null)		myObj = document.all(arrValues[index][0]);
		if(myObj==null)
		{	/*若为主键，则添加相应的隐藏控件，以保持它的值*/
			if(arrValues[index][2].toLowerCase() == 'true')
			{
				var newNode = document.createElement('input');
				newNode.type = 'hidden';
				newNode.value = value;
				newNode.name = clientID + '_' + arrValues[index][0];
				document.forms[0].insertBefore(newNode, document.forms[0].firstChild);
			}
		}

		if(arrValues[index][10] != '')	/*如果其中有格式化序列*/
		{
			if(value == '')	continue;
			var pattern  = arrValues[index][10];
			var myRegExp = new RegExp('\\{\\S+?\\}', 'mg');
			var patterns = pattern.match(myRegExp);
			var itemValues = value.split('[valueitem]');
			for(var k=0; k<itemValues.length; k++)
			{
				if(patterns[k]==null)	continue;
				var tagId = patterns[k].substring(1, patterns[k].length-1);
				var theObj = document.all(clientID + '_' + tagId);
				if(theObj==null)		theObj = document.all(tagId);
				if(theObj!=null)		setHtmlValue(theObj, itemValues[k], arrValues[index][9], isSetReadOnly);
			}
		}

		if(myObj==null)		continue;
		/*根据对象的类型给其赋值*/
		myObj.name = arrValues[index][0];
		var tagName = myObj.tagName;
		if(tagName != null)		/*为一个控件赋值*/
		{
			setHtmlValue(myObj, arrValues[index][1], arrValues[index][9], isSetReadOnly);
		}
		else	/*为一个控件数组赋值*/
		{
			var counter = myObj.length;
			var arrValue = arrValues[index][1].split(RegExp('\\s*,\\s*', 'img'));
			for(var k=0; k<counter; k++)
			{
				setHtmlValue(myObj[k], arrValue, arrValues[index][9], isSetReadOnly);
			}
		}
	}
}

/*为指定HTML元素赋值*/
function setHtmlValue(myObj, value, iSupportUBB, isSetReadOnly)
{
	var tagName = myObj.tagName.toUpperCase();
	switch(tagName)
	{
		case 'INPUT':	/*输入框*/
			switch(myObj.type.toUpperCase())
			{
				case 'HIDDEN': case 'PASSWORD': case 'TEXT':	/*隐藏控件， 密码框， 文本框*/
						myObj.value = value;	break;
				case 'IMAGE':									/*图片*/
						myObj.value = value;	myObj.src = value;		break;
				case 'FILE':	case 'BUTTON': case 'RESET': case 'SUBMIT':	/*上传文件控件， 按钮， 重置， 提交*/
						break;
				case 'CHECKBOX':	/*复选框*/
						if(typeof(value) == 'object')	/*如果是数组*/
						{
							for(var k=0; k<value.length; k++)
							{
								if(value[k].toUpperCase() == myObj.value.toUpperCase())		myObj.checked = true;
								else if(value[k]=='1' && myObj.value.toUpperCase()=='TRUE')	myObj.checked = true;
								else	continue;
								break;
							}
							if(k==value.length)		myObj.checked = false;
						}
						else
						{
							if(value.toUpperCase()==myObj.value.toUpperCase() || value.toUpperCase()=='TRUE'&&myObj.value=='1')	myObj.checked = true;
							else		myObj.checked = false;
						}
						break;
				case 'RADIO':		/*单选框*/
						if(typeof(value) == 'object')	/*如果是数组*/
						{
							for(var k=0; k<value.length; k++)
							{
								if(myObj.value.toUpperCase() == value[k].toUpperCase())		myObj.checked = true;
								else if(myObj.value=='1' && value[k].toUpperCase=='TRUE')	myObj.checked = true;
								else		continue;
								break;
							}
							if(k==value.length)	myObj.checked = false;
						}
						else
						{
							if(value.toUpperCase()==myObj.value.toUpperCase() || value.toUpperCase()=='TRUE'&&myObj.value=='1')	myObj.checked = true;
							else		myObj.checked = false;
						}
						break;
				default: myObj.value = value;					/*其它*/
			}
			break;
		case 'SELECT':		/*列表框*/
			myObj.value = value;
			break;
		case 'TEXTAREA':	/*文本区域*/
			myObj.value = value;
			break;
		case 'IMG':			/*图片*/
			myObj.src = value;
			break;
		default:			/*其它*/
			try
			{
				if(myObj.value!=null)	myObj.value = value;
				else
				{	/*根据是否支持UBB决定如何显示*/
					if(iSupportUBB=='False')
						myObj.innerHTML = value.replace(RegExp('&','img'), '&amp;').replace(RegExp('<', 'img'), '&lt;').replace(RegExp('>', 'img'), '&gt;')
											.replace(RegExp(' ', 'img'), '&nbsp;').replace(RegExp('""', 'img'), '&quot;').replace(RegExp('\n','img'), '<br>');
					else
						myObj.innerHTML = value.replace(RegExp('\n', 'img'), '<br>');
				}
			}
			catch(e) {}
	}
	if(isSetReadOnly && value!='')		/*将主键的HTML元素设置为只读形式*/
	{
		if(tagName=='INPUT' || tagName=='TEXTAREA' || tagName=='SELECT')
			try { myObj.readOnly = true; } catch(e){}
	}
}
",
"ReportDetail.Js");

		#endregion

		#region 客户端对菜单操作的一些脚本
		
		/// <summary>
		/// 对汇总屏脚本的访问
		/// </summary>
		static public string ReportMenuOperateScript
		{
			get
			{
				return _ReportMenuOperateScript.Script;
			}
		}

		private static ScriptOperate _ReportMenuOperateScript = new ScriptOperate(
@"
function ReportMenu(menuArr, _width)
{
	var myDiv = document.createElement('div');
	with(myDiv.style)
	{
		top = event.y;
		left = event.x;
		fontSize = 12;
		position = 'absolute';
		backgroundColor = '#d4d0c8';
		cursor = 'default';
		borderWidth = 1;
		borderStyle = 'outset';
		paddingTop = paddingBottom = 2;
		color = 'black';
		if(width!=null)	width = _width;
	}

	var id = ('MyReportMenu_' + Math.random()).replace('.', '');
	myDiv.id = id;
	myDiv.onselectstart = function() { event.returnValue = false; };

	// 显示菜单
	this.Show = function()
	{
		for(var k=0; k<menuArr.length; k++)
		{
			var item = document.createElement('div');
			var text = menuArr[k][0];
			var func = menuArr[k][1];
			item.func = func;
			if(text=='-')	item.innerHTML = '<nobr><hr\twidth=100%></nobr>';
			else
			{
				item.innerHTML = '<nobr>' + text + '<nobr>';
				with(item.style)
				{
					paddingLeft = paddingRight  = 10;
					paddingTop    =  paddingBottom = 2;
					width = '100%';
				}
				item.onmouseenter = function() { with(this.style) { backgroundColor = '#0a246a'; color='#ffffff'; } };
				item.onmouseleave = function() { with(this.style) { backgroundColor = '#d4d0c8'; color='#000000'; } };
				item.onmouseup	  = function() { HideMenu();  if(this.func!=''&&this.func!=null)  window.eval(this.func);  };
			}
			myDiv.appendChild(item);
		}
		document.body.appendChild(myDiv);
		document.attachEvent('onmousedown', HideMenu);
		if(event!=null)	event.returnValue = false;
	};
	
	// 隐藏菜单
	this.Hide = function() { HideMenu(); };

	HideMenu = function()
	{
		var menu = document.all(id);
		if(menu==null)	return;
		if(event!=null && event.type=='mousedown')
		{
			// 当单击到选项上时不执行任何操作
			var node = event.srcElement;
			while(node!=null) { if(node==menu) break; node = node.parentNode; }
			if(node!=null)	return;
		}
		document.body.removeChild(menu);
		document.detachEvent('onmousedown', HideMenu);
	};
}
",
"ReportMenu.Js");
		#endregion

		#region 对客户端脚本的一些操作

		/// <summary>
		/// 对脚本的一些操作
		/// </summary>
		private class ScriptOperate
		{
			/// <summary>
			/// 构造函数
			/// </summary>
			/// <param name="ScriptString"></param>
			public ScriptOperate(string ScriptString, string FileName)
			{
				this.ScriptString = ScriptString;
				this.FileName = FileName;
			}

			private string ScriptString = null;
			private bool IsUpdate = false;
			private string FileName = null;
			private bool IsSend = false;

			/// <summary>
			/// 对脚本的访问
			/// </summary>
			public string Script
			{
				get
				{
					if(!this.IsUpdate)
					{
						this.IsUpdate = true;
						this.ScriptString = ReplaceScript.Replace(this.ScriptString, "");
						if(Option.IsSendScriptFile)		this.IsSend = this.SendScriptFile();

						if(this.IsSend) ScriptString = string.Concat("<script type=text/javascript src='", ReportBase.ScriptPath, @"\", FileName, "'></script>");
						else			ScriptString = string.Concat("<script language=javascript>\n<!--\n", ScriptString, "\n//-->\n</script>\n");
					}
					return ScriptString;
				}
			}

			/// <summary>
			/// 用于将脚本的空格去除
			/// </summary>
			static private Regex ReplaceScript = new Regex(@"//[\s\S]*?\r\n|/\*[\s\S]*?\*/|(?<!else|function|return|var|new)\s+", RegexOptions.Multiline);

			/// <summary>
			/// 向客户端发送脚本
			/// </summary>
			/// <param name="FileName"></param>
			/// <param name="Script"></param>
			/// <returns></returns>
			private bool SendScriptFile()
			{
				string Path = System.Web.HttpContext.Current.Server.MapPath(ReportBase.ScriptPath) + @"\" + FileName;
				try
				{
					System.IO.StreamWriter myFile = System.IO.File.CreateText(Path);
					myFile.Write(ScriptString);
					myFile.Close();
				}
				catch
				{
					if(!System.IO.File.Exists(Path))		return false;
				}
				return true;
			}
		}

		#endregion
	}
}
