package com.game.gow.module.role.service;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.game.gow.module.equip.service.EquipService;
import com.game.gow.module.role.manager.Pet;
import com.game.gow.module.role.manager.PetManager;
import com.game.gow.module.role.model.RoleVo;

/**
 * 宠物服务
 */
@Service
public class PetService {


	@Autowired
	private PetManager petManager;
	
	@Autowired
	private EquipService equipService;

	
	/**
	 * 获取宠物列表
	 * @param accountId 玩家标识
	 * @return
	 */
	public List<RoleVo> getPetList( long accountId )
	{
		Collection<Pet> pets = petManager.loadByOwner(accountId);
		ArrayList<RoleVo> result = new ArrayList<RoleVo>();
		for( Pet pet : pets )
		{
			result.add(RoleVo.valueOf(pet));
		}
		return result;
	}
	
	void createOneTestPet( long accountId, String petId )
	{
		Pet pet = new Pet();
		pet.setCfgId(petId);
		pet.setEquips(equipService.getInitEquips(petId));
		pet.setOwner(accountId);
		pet.setAdvanceLevel(1);
		pet.setStar(1);
		petManager.create(pet);
	}
	
	public void createTestPets( long accountId )
	{
		Collection<Pet> pets = petManager.loadByOwner(accountId);
		for(Pet pet : pets) // 暂时修正等阶等信息
		{
			if(pet.getAdvanceLevel()<1)
			{
				pet.setAdvanceLevel(1);
			}
			if(pet.getStar()<1)
			{
				pet.setStar(1);
			}
		}
	
		if(pets.size() < 3)
		{
			for(Pet pet : pets)
			{
				petManager.remove(pet);
			}
			
			createOneTestPet(accountId, "chongwu1");
			createOneTestPet(accountId, "chongwu2");
			createOneTestPet(accountId, "kuangzhanshi");
		}
	}
}
