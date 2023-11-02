// Import the Net library so we can create a server to talk with the C# part of the debug adapter
import * as Net from 'net';
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

    // Write out events sent from the debug adapter
    context.subscriptions.push(vscode.debug.onDidReceiveDebugSessionCustomEvent((e) => {
		console.log(e.event);
	}));

    context.subscriptions.push(vscode.debug.registerDebugAdapterDescriptorFactory('stepbro', new StepBroDebugAdapterExecutableDescriptorFactory())); // This maybe??

    // vscode.debug.startDebugging(vscode.workspace.workspaceFolders != undefined ? vscode.workspace.workspaceFolders[0] : undefined, undefined as unknown as vscode.DebugConfiguration);
}

// This method is called when your extension is deactivated
export function deactivate() {}

class StepBroDebugAdapterExecutableDescriptorFactory implements vscode.DebugAdapterDescriptorFactory
{
    createDebugAdapterDescriptor(session: vscode.DebugSession, executable: vscode.DebugAdapterExecutable | undefined): vscode.ProviderResult<vscode.DebugAdapterDescriptor>
    {
        return new vscode.DebugAdapterExecutable("c:/SW_development/StepBro/additional/stepbro-vscode-debug-adapter/bin/Debug/stepbro-debug-adapter.exe"); // TODO: Update with a more generic name eventually
    }

    dispose()
    {
        // Do nothing
    }
}