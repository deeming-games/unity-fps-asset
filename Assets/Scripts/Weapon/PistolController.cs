namespace Weapon
{
	public class PistolController : WeaponControllerBase
	{
		private protected override void PlayShootAnimation()
		{
			Animator.Play(BulletsLeft > 1 ? "shoot_not_empty" : "shoot_empty");
		}
		
		private protected override void PlayReloadAnimation()
		{
			Animator.Play(BulletsLeft > 1 ? "reload_not_empty" : "reload_empty");
		}
		
		private protected override void PlayDrawAnimation()
		{
			Animator.Play(BulletsLeft > 0 ? "draw_not_empty" : "draw_empty");
		}
		
		private protected override void PlayHolsterAnimation()
		{
			Animator.Play(BulletsLeft > 0 ? "holster_not_empty" : "holster_empty");
		}
	}
}