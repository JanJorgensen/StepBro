"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.deactivate = exports.activate = void 0;
// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
const vscode = require("vscode");
// This method is called when your extension is activated
// Your extension is activated the very first time the command is executed
function activate(context) {
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
//# sourceMappingURL=extension.js.map