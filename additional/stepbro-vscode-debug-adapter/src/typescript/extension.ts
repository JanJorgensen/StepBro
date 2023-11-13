// Import the Net library so we can create a server to talk with the C# part of the debug adapter
import * as Net from 'net';
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';

// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) 
{
    // Register a command that gets program name for people using that launch setup
    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.getProgramName', config => {
		return vscode.window.showInputBox({
			placeHolder: "Please enter the name of a stepbro file in the workspace folder",
			value: "ConsoleTest.sbs"
		});
	}));

    // Set up the 5 event handlers defined in the API
    // https://code.visualstudio.com/api/references/vscode-api#debug
    context.subscriptions.push(vscode.debug.onDidChangeActiveDebugSession((e) => {
        console.log("Active Debug Session Change: " + e?.name); // Just to have an implementation
    }));

    context.subscriptions.push(vscode.debug.onDidChangeBreakpoints((e) => {
        console.log("Breakpoint change: " + e);
    }));

    // Custom events are currently used for debugging
    context.subscriptions.push(vscode.debug.onDidReceiveDebugSessionCustomEvent((e) => {
		console.log("Custom event: " + e.event);
	}));

    context.subscriptions.push(vscode.debug.onDidStartDebugSession((e) => {
        console.log("Start Debug Session: " + e.name);
    }));

    context.subscriptions.push(vscode.debug.onDidTerminateDebugSession((e) => {
        console.log("Terminate Debug Session: " + e.name);
    }));

    // Register the stepbro debug adapter as an executable (So we can write it in C#)
    context.subscriptions.push(vscode.debug.registerDebugAdapterDescriptorFactory('stepbro', new StepBroDebugAdapterExecutableDescriptorFactory()));

    context.subscriptions.push(vscode.debug.registerDebugAdapterTrackerFactory('stepbro', new StepBroDebugAdapterTrackerFactory()));

    // Add a command to run or debug the current file
    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.runEditorContents', (resource: vscode.Uri) => {
        let targetResource = resource;
        if (!targetResource && vscode.window.activeTextEditor) {
            targetResource = vscode.window.activeTextEditor.document.uri;
        }
        if (targetResource) {
            vscode.debug.startDebugging(undefined, {
                type: 'stepbro',
                name: 'Run File',
                request: 'launch',
                program: targetResource.fsPath
            },
                { noDebug: true }
            );
        }
        }),
        vscode.commands.registerCommand('extension.stepbro-vscode-debug.debugEditorContents', (resource: vscode.Uri) => {
        let targetResource = resource;
        if (!targetResource && vscode.window.activeTextEditor) {
            targetResource = vscode.window.activeTextEditor.document.uri;
        }
        if (targetResource) {
            vscode.debug.startDebugging(undefined, {
                type: 'stepbro',
                name: 'Debug File',
                request: 'launch',
                program: targetResource.fsPath
            });
        }
    }));
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

class StepBroDebugAdapterTrackerFactory implements vscode.DebugAdapterTrackerFactory
{
    createDebugAdapterTracker(session: vscode.DebugSession): vscode.ProviderResult<vscode.DebugAdapterTracker>
    {
        return {
            onWillReceiveMessage: m => console.log(`Received: ${JSON.stringify(m, undefined, 2)}`),
            onDidSendMessage: m => console.log(`Sent: ${JSON.stringify(m, undefined, 2)}`)
        }
    }
}