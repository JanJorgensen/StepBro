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

    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.startSession', config => startSession(config)));

    context.subscriptions.push(vscode.debug.registerDebugAdapterDescriptorFactory('stepbro', new StepBroDebugAdapterExecutableDescriptorFactory())); // This maybe??

    console.log("Executable pushed");
}

// This method is called when your extension is deactivated
export function deactivate() {}

function startSession(config: any)
{
    console.log("Trying to start a session...");

    vscode.debug.startDebugging((vscode.workspace.workspaceFolders != undefined ? vscode.workspace.workspaceFolders[0] : undefined), config)
    .then(test => {
        console.log((vscode.workspace.workspaceFolders != undefined ? vscode.workspace.workspaceFolders[0] : undefined));
        console.log(config);
        console.log("Session started!");
    })
    .then(undefined, err => {
        console.log("ERROR: " + err);
    });
}

class StepBroDebugAdapterExecutableDescriptorFactory implements vscode.DebugAdapterDescriptorFactory
{
    private server?: Net.Server;

    createDebugAdapterDescriptor(session: vscode.DebugSession, executable: vscode.DebugAdapterExecutable | undefined): vscode.ProviderResult<vscode.DebugAdapterDescriptor>
    {
        return new vscode.DebugAdapterExecutable("c:/SW_development/StepBro/additional/stepbro-vscode-debug-adapter/bin/Debug/stepbro-debug-adapter.exe"); // TODO: Update with a more generic name eventually
    }

    dispose()
    {
        // Do nothing
    }
}