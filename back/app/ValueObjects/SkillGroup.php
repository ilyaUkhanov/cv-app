<?php

namespace App\ValueObjects;

class SkillGroup
{
    /**
     * @param  SkillItem[]  $items
     */
    public function __construct(
        public string $group,
        public array $items = [],
        public int $position = 0
    ) {}

    public function toArray(): array
    {
        return [
            'group' => $this->group,
            'items' => array_map(fn (SkillItem $item) => $item->toArray(), $this->items),
            'position' => $this->position,
        ];
    }

    public static function fromArray(array $data): self
    {
        return new self(
            group: $data['group'],
            items: array_map(
                fn (array $item) => SkillItem::fromArray($item),
                $data['items'] ?? []
            ),
            position: $data['position'] ?? 0
        );
    }
}
