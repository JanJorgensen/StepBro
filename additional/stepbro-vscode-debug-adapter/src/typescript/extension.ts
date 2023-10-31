// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';

// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) 
{
    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.getProgramName', config => {
		return vscode.window.showInputBox({
			placeHolder: "Please enter the name of a stepbro file in the workspace folder",
			value: "ConsoleTest.sbs"
		});
	}));

    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.startSession', config => startSession(config)));
}

// This method is called when your extension is deactivated
export function deactivate() {}

function startSession(config: any)
{
    console.log("We started a session!");
	vscode.commands.executeCommand('vscode.startDebug', config);
}