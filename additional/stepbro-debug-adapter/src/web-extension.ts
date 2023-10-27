/*---------------------------------------------------------
 * Copyright (C) Microsoft Corporation. All rights reserved.
 *--------------------------------------------------------*/
/*
 * web-extension.ts (and activateStepBroDebug.ts) forms the "plugin" that plugs into VS Code and contains the code that
 * connects VS Code with the debug adapter.
 * 
 * web-extension.ts launches the debug adapter "inlined" because that's the only supported mode for running the debug adapter in the browser.
 */

import * as vscode from 'vscode';
import { activateStepBroDebug } from './activateStepBroDebug';

export function activate(context: vscode.ExtensionContext) {
	activateStepBroDebug(context);	// activateStepBroDebug without 2nd argument launches the Debug Adapter "inlined"
}

export function deactivate() {
	// nothing to do
}
