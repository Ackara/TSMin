namespace Foo {
	export class PredatorBase {
		name?: string;
	}

	export interface SkillBase<any> {
	}

	export class AfricanLionBase extends PredatorBase, SkillBase<any> {
		region?: string;
	}
}
