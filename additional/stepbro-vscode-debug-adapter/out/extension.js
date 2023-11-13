"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.deactivate = exports.activate = void 0;
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
const vscode = require("vscode");
// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
function activate(context) {
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
    context.subscriptions.push(vscode.commands.registerCommand('extension.stepbro-vscode-debug.runEditorContents', (resource) => {
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
            }, { noDebug: true });
        }
    }), vscode.commands.registerCommand('extension.stepbro-vscode-debug.debugEditorContents', (resource) => {
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
exports.activate = activate;
// This method is called when your extension is deactivated
function deactivate() { }
exports.deactivate = deactivate;
class StepBroDebugAdapterExecutableDescriptorFactory {
    createDebugAdapterDescriptor(session, executable) {
        return new vscode.DebugAdapterExecutable("c:/SW_development/StepBro/additional/stepbro-vscode-debug-adapter/bin/Debug/stepbro-debug-adapter.exe"); // TODO: Update with a more generic name eventually
    }
    dispose() {
        // Do nothing
    }
}
class StepBroDebugAdapterTrackerFactory {
    createDebugAdapterTracker(session) {
        return {
            onWillReceiveMessage: m => console.log(`Received: ${JSON.stringify(m, undefined, 2)}`),
            onDidSendMessage: m => console.log(`Sent: ${JSON.stringify(m, undefined, 2)}`)
        };
    }
}
//# sourceMappingURL=extension.js.map